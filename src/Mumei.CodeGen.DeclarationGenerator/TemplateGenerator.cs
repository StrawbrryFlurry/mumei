using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Mumei.CodeGen.DeclarationGenerator;

/// <summary>
/// Processes <c>partial record</c>s deriving from <c>ClassTemplate</c> / <c>InterceptorTemplate</c> in the generator project.
/// For each template, it generates the method that emits the template members, input-based equality and,
/// for generic templates, a non-generic sibling record to instantiate the template with.
/// </summary>
[Generator]
public sealed class TemplateGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var templates = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is RecordDeclarationSyntax { BaseList: not null } record
                                && !record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword),
            static (ctx, ct) => TemplateEmitter.Emit((RecordDeclarationSyntax)ctx.Node, ctx.SemanticModel, ct)
        ).Where(static x => x is not null);

        context.RegisterSourceOutput(templates, static (context, template) => {
            foreach (var diagnostic in template!.Diagnostics) {
                context.ReportDiagnostic(diagnostic.ToDiagnostic());
            }

            if (template.Source is not null) {
                context.AddSource(template.HintName, template.Source);
            }
        });
    }
}

internal static class TemplateEmitter {
    private const string TypeRef = "global::Mumei.CodeGen.TypeRef";

    public static TemplateResult? Emit(RecordDeclarationSyntax node, SemanticModel model, CancellationToken ct) {
        if (model.GetDeclaredSymbol(node, ct) is not { IsAbstract: false } template || GetTemplateKind(template) is not { } kind) {
            return null;
        }

        var declarations = template.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax(ct))
            .OfType<RecordDeclarationSyntax>()
            .ToArray();
        if (declarations.FirstOrDefault(d => d.BaseList is not null) != node) {
            return null; // Handled through the first declaration
        }

        var hintName = template.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "")
            .Replace('<', '[')
            .Replace('>', ']') + ".Template.g.cs";
        var diagnostics = new List<DiagnosticInfo>();

        if (!node.Modifiers.Any(SyntaxKind.PartialKeyword)) {
            diagnostics.Add(DiagnosticInfo.Create(TemplateDiagnostics.NotPartial, node.Identifier.GetLocation(), template.Name));
            return new TemplateResult(hintName, null, diagnostics.ToArray());
        }

        if (template.ContainingType is not null) {
            diagnostics.Add(DiagnosticInfo.Create(TemplateDiagnostics.Nested, node.Identifier.GetLocation(), template.Name));
            return new TemplateResult(hintName, null, diagnostics.ToArray());
        }

        var bindings = BindTypeParameters(template, diagnostics);
        var interceptMethod = kind == TemplateKind.Interceptor
            ? template.GetMembers("Intercept").OfType<IMethodSymbol>().FirstOrDefault(m => m.IsOverride && m.Parameters.Length == 1)
            : null;
        var staging = new TemplateStaging(template, bindings, interceptMethod, diagnostics);

        var emit = new List<string>();
        var stubs = new List<string>();
        var generationTimeMembers = new List<MemberDeclarationSyntax>();

        foreach (var declaration in declarations) {
            var semanticModel = declaration.SyntaxTree == model.SyntaxTree ? model : model.Compilation.GetSemanticModel(declaration.SyntaxTree);
            if (kind == TemplateKind.Class) {
                EmitInterfaces(declaration, semanticModel, staging, emit);
            }

            foreach (var member in declaration.Members) {
                ct.ThrowIfCancellationRequested();
                var symbol = GetDeclaredSymbol(member, semanticModel, ct);
                if (symbol is null) {
                    continue;
                }

                if (kind == TemplateKind.Interceptor) {
                    if (SymbolEqualityComparer.Default.Equals(symbol, interceptMethod)) {
                        EmitInterceptBody((MethodDeclarationSyntax)member, semanticModel, staging, emit);
                    } else if (TemplateSymbols.IsGenerationTimeMember(symbol)) {
                        generationTimeMembers.Add(member);
                    } else {
                        diagnostics.Add(DiagnosticInfo.Create(TemplateDiagnostics.InterceptorMemberIgnored, symbol.Locations.FirstOrDefault(), symbol.Name));
                    }

                    continue;
                }

                if (TemplateSymbols.IsGenerationTimeMember(symbol)) {
                    generationTimeMembers.Add(member);
                    continue;
                }

                if (symbol is IMethodSymbol method && TemplateSymbols.HasAttribute(method, "GeneratedAttribute")) {
                    EmitGeneratedMethod((MethodDeclarationSyntax)member, method, semanticModel, staging, emit, stubs);
                    continue;
                }

                var staged = StagingRewriter.Stage(semanticModel, staging, member);
                var text = Dedent(Indentation(member) + staged.WithoutLeadingTrivia().ToFullString());
                emit.Add($"__self.Member({staging.ToWriterExpression(text)});");
            }
        }

        var source = new SourceWriter();
        source.Line("// <auto-generated/>");
        source.Line("#nullable enable");
        source.Line("#pragma warning disable CS1591");
        foreach (var @using in GetUsings(node, outsideNamespace: true)) {
            source.Line(@using);
        }

        var ns = template.ContainingNamespace.IsGlobalNamespace ? null : template.ContainingNamespace.ToDisplayString();
        if (ns is not null) {
            source.Line();
            source.Open($"namespace {ns}");
            foreach (var @using in GetUsings(node, outsideNamespace: false)) {
                source.Line(@using);
            }
        }

        var typeParameters = template.TypeParameters.Length == 0 ? "" : $"<{string.Join(", ", template.TypeParameters.Select(t => t.Name))}>";
        source.Line();
        source.Open($"partial record {template.Name}{typeParameters}");
        foreach (var stub in stubs) {
            source.Lines(stub);
        }

        if (template.IsGenericType) {
            source.Close();
            source.Line();
            EmitSibling(source, template, kind, generationTimeMembers, emit, staging);
        } else {
            EmitTemplateMembers(source, template, kind, template.IsSealed, GetInputs(template).Select(p => p.Name), emit, staging);
            source.Close();
        }

        if (ns is not null) {
            source.Close();
        }

        return new TemplateResult(hintName, source.ToString(), diagnostics.ToArray());
    }

    private enum TemplateKind {
        Class,
        Interceptor
    }

    private static TemplateKind? GetTemplateKind(INamedTypeSymbol type) {
        for (var current = type.BaseType; current is not null; current = current.BaseType) {
            if (TemplateSymbols.IsMumeiType(current, "ClassTemplate")) {
                return TemplateKind.Class;
            }

            if (TemplateSymbols.IsMumeiType(current, "InterceptorTemplate")) {
                return TemplateKind.Interceptor;
            }
        }

        return null;
    }

    private static Dictionary<ITypeParameterSymbol, string> BindTypeParameters(INamedTypeSymbol template, List<DiagnosticInfo> diagnostics) {
        var bindings = new Dictionary<ITypeParameterSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var typeParameter in template.TypeParameters) {
            var input = GetInputs(template).FirstOrDefault(p => GetBoundTypeParameter(p) is { } bound
                                                               && SymbolEqualityComparer.Default.Equals(bound, typeParameter));
            if (input is null) {
                diagnostics.Add(DiagnosticInfo.Create(
                    TemplateDiagnostics.UnboundTypeParameter,
                    typeParameter.Locations.FirstOrDefault(),
                    typeParameter.Name,
                    typeParameter.Name.TrimStart('T') is { Length: > 0 } n ? n : "Type"
                ));
                continue;
            }

            bindings[typeParameter] = $"this.{input.Name}";
        }

        return bindings;
    }

    private static ITypeSymbol? GetBoundTypeParameter(IPropertySymbol property) {
        return property.Type is INamedTypeSymbol { TypeArguments.Length: 1 } type && TemplateSymbols.IsMumeiType(type, "Type")
            ? type.TypeArguments[0]
            : null;
    }

    private static IEnumerable<IPropertySymbol> GetInputs(INamedTypeSymbol template) {
        return template.GetMembers().OfType<IPropertySymbol>().Where(p => p is { IsRequired: true, IsStatic: false });
    }

    private static ISymbol? GetDeclaredSymbol(MemberDeclarationSyntax member, SemanticModel model, CancellationToken ct) {
        return member switch {
            BaseFieldDeclarationSyntax field => field.Declaration.Variables.FirstOrDefault() is { } variable
                ? model.GetDeclaredSymbol(variable, ct)
                : null,
            _ => model.GetDeclaredSymbol(member, ct)
        };
    }

    private static void EmitInterfaces(RecordDeclarationSyntax declaration, SemanticModel model, TemplateStaging staging, List<string> emit) {
        foreach (var baseType in declaration.BaseList?.Types ?? default) {
            if (model.GetTypeInfo(baseType.Type).Type is not { TypeKind: TypeKind.Interface }) {
                continue;
            }

            var staged = StagingRewriter.Stage(model, staging, baseType.Type).WithoutTrivia().ToFullString();
            var expression = staging.ToWriterExpression(staged);
            emit.Add($"__self.Implements({TypeRef}.Named({(expression.StartsWith("\"") ? expression : expression + ".ToString()")}));");
        }
    }

    private static void EmitInterceptBody(MethodDeclarationSyntax method, SemanticModel model, TemplateStaging staging, List<string> emit) {
        string text;
        if (method.Body is { } body) {
            var staged = (BlockSyntax)StagingRewriter.Stage(model, staging, body);
            text = Dedent(string.Concat(staged.Statements.Select(s => s.ToFullString())));
        } else if (method.ExpressionBody is { } arrow) {
            var staged = StagingRewriter.Stage(model, staging, arrow.Expression).WithoutTrivia().ToFullString();
            // `=> call.Proceed()` is staged as `ReturnProceed` already
            text = staged.StartsWith("__mumei_hole_", StringComparison.Ordinal) && IsReturnProceed(arrow.Expression, model)
                ? staged
                : $"return {staged};";
        } else {
            return;
        }

        emit.Add($"__i.Body({staging.ToWriterExpression(text)});");
    }

    private static bool IsReturnProceed(ExpressionSyntax expression, SemanticModel model) {
        return expression is InvocationExpressionSyntax
               && model.GetSymbolInfo(expression).Symbol is IMethodSymbol { Name: "Proceed" } method
               && TemplateSymbols.IsMumeiType(method.ContainingType, "Call");
    }

    private static void EmitGeneratedMethod(
        MethodDeclarationSyntax method,
        IMethodSymbol symbol,
        SemanticModel model,
        TemplateStaging staging,
        List<string> emit,
        List<string> stubs
    ) {
        if (!method.Modifiers.Any(SyntaxKind.PartialKeyword) || method.Body is not null || method.ExpressionBody is not null) {
            staging.Diagnostics.Add(DiagnosticInfo.Create(TemplateDiagnostics.GeneratedMethodNotPartial, method.Identifier.GetLocation(), symbol.Name));
            return;
        }

        var staged = (MethodDeclarationSyntax)StagingRewriter.Stage(
            model,
            staging,
            method,
            attribute => model.GetSymbolInfo(attribute).Symbol is IMethodSymbol ctor
                         && TemplateSymbols.IsMumeiType(ctor.ContainingType, "GeneratedAttribute")
        );
        staged = staged
            .WithModifiers(TokenList(staged.Modifiers.Where(m => !m.IsKind(SyntaxKind.PartialKeyword))))
            .WithSemicolonToken(MissingToken(SyntaxKind.SemicolonToken));
        var signature = Dedent(Indentation(method) + staged.WithoutLeadingTrivia().WithoutTrailingTrivia().ToFullString());
        emit.Add($"__self.GeneratedMethod({TemplateStaging.Literal(symbol.Name)}, {staging.ToWriterExpression(signature)});");

        // The implementing declaration lets the template compile in the generator project; the real body comes from Build
        var stub = method
            .WithAttributeLists(default)
            .WithParameterList(method.ParameterList.WithParameters(SeparatedList(method.ParameterList.Parameters.Select(p => p.WithDefault(null)))))
            .WithSemicolonToken(MissingToken(SyntaxKind.SemicolonToken))
            .WithExpressionBody(ArrowExpressionClause(Token(SyntaxKind.EqualsGreaterThanToken).WithLeadingTrivia(Space).WithTrailingTrivia(Space), ParseExpression(
                $"throw new global::System.NotSupportedException(\"[Generated] methods are implemented in the generated code through Build.\")"
            )))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            .WithoutLeadingTrivia()
            .WithoutTrailingTrivia();
        stubs.Add(Dedent(Indentation(method) + stub.ToFullString()));
    }

    private static void EmitSibling(
        SourceWriter source,
        INamedTypeSymbol template,
        TemplateKind kind,
        List<MemberDeclarationSyntax> generationTimeMembers,
        List<string> emit,
        TemplateStaging staging
    ) {
        var accessibility = template.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
        var baseType = template.BaseType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        source.Line($"/// <summary>Instantiates the generic template <see cref=\"{template.Name}{{{string.Join(", ", template.TypeParameters.Select(t => t.Name))}}}\"/>.</summary>");
        source.Open($"{accessibility} sealed record {template.Name} : {baseType}");

        foreach (var member in generationTimeMembers) {
            var copy = member;
            if (member is PropertyDeclarationSyntax property
                && GetInputs(template).FirstOrDefault(p => p.Name == property.Identifier.ValueText) is { } input
                && GetBoundTypeParameter(input) is not null) {
                copy = property.WithType(ParseTypeName(TypeRef).WithTriviaFrom(property.Type));
            }

            source.Lines(Dedent(Indentation(member) + copy.WithoutLeadingTrivia().WithoutTrailingTrivia().ToFullString()));
            source.Line();
        }

        if (kind == TemplateKind.Interceptor && staging.InterceptMethod is { } intercept) {
            var typeParameter = intercept.TypeParameters.FirstOrDefault()?.Name ?? "TResult";
            source.Line(
                $"public override {typeParameter} Intercept<{typeParameter}>(global::Mumei.CodeGen.Call<{typeParameter}> {intercept.Parameters[0].Name}) "
                + "=> throw new global::System.NotSupportedException();"
            );
            source.Line();
        }

        var inputs = generationTimeMembers.OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.Any(SyntaxKind.RequiredKeyword))
            .Select(p => p.Identifier.ValueText);
        EmitTemplateMembers(source, template, kind, isSealed: true, inputs, emit, staging);
        source.Close();
    }

    private static void EmitTemplateMembers(
        SourceWriter source,
        INamedTypeSymbol template,
        TemplateKind kind,
        bool isSealed,
        IEnumerable<string> inputs,
        List<string> emit,
        TemplateStaging staging
    ) {
        if (kind == TemplateKind.Class) {
            source.Open("protected override void EmitMembers(global::Mumei.CodeGen.TypeBuilder __self)");
        } else {
            var call = staging.CallParameter?.Name ?? "call";
            source.Open($"protected override void EmitInterceptor(global::Mumei.CodeGen.InterceptorBuilder __i, global::Mumei.CodeGen.Call<object?> {call})");
        }

        foreach (var statement in emit) {
            source.Lines(statement);
        }

        source.Close();

        var hasCustomEquality = template.GetMembers("Equals").OfType<IMethodSymbol>().Any(m =>
            !m.IsImplicitlyDeclared && m.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type.OriginalDefinition, template)
        );
        if (hasCustomEquality) {
            return;
        }

        // Templates are compared by their inputs only: emitted fields (e.g. `List<T> _items = new()`) exist on the
        // template instance too, but must not affect caching.
        var inputList = inputs.ToArray();
        source.Line();
        source.Open($"public {(isSealed ? "" : "virtual ")}bool Equals({template.Name}? other)");
        source.Line("return other is not null");
        source.Line("       && EqualityContract == other.EqualityContract" + (inputList.Length == 0 ? ";" : ""));
        for (var i = 0; i < inputList.Length; i++) {
            source.Line($"       && global::System.Object.Equals(this.{inputList[i]}, other.{inputList[i]}){(i == inputList.Length - 1 ? ";" : "")}");
        }
        source.Close();
        source.Line();
        source.Open("public override int GetHashCode()");
        source.Line("var hash = new global::System.HashCode();");
        source.Line("hash.Add(EqualityContract);");
        foreach (var input in inputList) {
            source.Line($"hash.Add(this.{input});");
        }

        source.Line("return hash.ToHashCode();");
        source.Close();
        source.Line();
        source.Open("protected override bool PrintMembers(global::System.Text.StringBuilder builder)");
        for (var i = 0; i < inputList.Length; i++) {
            source.Line($"builder.Append({TemplateStaging.Literal((i > 0 ? ", " : "") + inputList[i] + " = ")}).Append((object?)this.{inputList[i]});");
        }

        source.Line("return true;");
        source.Close();
    }

    private static IEnumerable<string> GetUsings(SyntaxNode node, bool outsideNamespace) {
        if (outsideNamespace) {
            return node.SyntaxTree.GetCompilationUnitRoot().Usings
                .Where(u => u.GlobalKeyword.IsKind(SyntaxKind.None))
                .Select(u => u.WithoutTrivia().ToFullString());
        }

        return node.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse()
            .SelectMany(n => n.Usings)
            .Select(u => u.WithoutTrivia().ToFullString());
    }

    private static string Indentation(SyntaxNode node) {
        return new string(' ', node.GetLocation().GetLineSpan().StartLinePosition.Character);
    }

    /// <summary>Removes surrounding blank lines and the indentation shared by all lines.</summary>
    internal static string Dedent(string text) {
        var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0])) {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1])) {
            lines.RemoveAt(lines.Count - 1);
        }

        var indentation = lines.Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Length - l.TrimStart().Length)
            .DefaultIfEmpty(0)
            .Min();
        return string.Join("\n", lines.Select(l => (l.Length >= indentation ? l[indentation..] : l.TrimStart()).TrimEnd()));
    }
}

internal sealed class SourceWriter {
    private readonly StringBuilder _text = new();
    private int _indentation;

    public void Line(string line = "") {
        if (line.Length > 0) {
            _text.Append(' ', _indentation * 4);
        }

        _text.Append(line).Append('\n');
    }

    public void Lines(string lines) {
        foreach (var line in lines.Split('\n')) {
            Line(line);
        }
    }

    public void Open(string header) {
        Line(header + " {");
        _indentation++;
    }

    public void Close() {
        _indentation--;
        Line("}");
    }

    public override string ToString() {
        return _text.ToString();
    }
}
