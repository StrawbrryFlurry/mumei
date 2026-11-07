using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Mumei.CodeGen.DeclarationGenerator;

internal enum HoleKind {
    /// <summary>A generation-time value: literals for strings / numbers / enums, names for types.</summary>
    Value,

    /// <summary>A generation-time value replacing an interpolation <c>{x}</c> in a regular interpolated string.</summary>
    InterpolatedValue,

    /// <summary>An expression evaluating to <c>Code</c>.</summary>
    Code
}

internal readonly record struct Hole(HoleKind Kind, string Expression);

/// <summary>State shared by all members of a template while staging them.</summary>
internal sealed class TemplateStaging(
    INamedTypeSymbol template,
    Dictionary<ITypeParameterSymbol, string> typeParameterBindings,
    IMethodSymbol? interceptMethod,
    List<DiagnosticInfo> diagnostics
) {
    private const string MarkerPrefix = "__mumei_hole_";
    private static readonly Regex MarkerPattern = new(@"__mumei_hole_(\d+)__", RegexOptions.Compiled);

    private readonly List<Hole> _holes = [];
    private int _writers;

    public INamedTypeSymbol Template { get; } = template;
    public Dictionary<ITypeParameterSymbol, string> TypeParameterBindings { get; } = typeParameterBindings;
    public IMethodSymbol? InterceptMethod { get; } = interceptMethod;
    public IParameterSymbol? CallParameter { get; } = interceptMethod?.Parameters.FirstOrDefault();
    public List<DiagnosticInfo> Diagnostics { get; } = diagnostics;

    public string AddHole(HoleKind kind, string expression) {
        _holes.Add(new Hole(kind, expression));
        return $"{MarkerPrefix}{_holes.Count - 1}__";
    }

    /// <summary>
    /// Turns text containing hole markers into a C# expression producing the code, e.g.
    /// <c>TemplateWriter.Build(w => w.Text("return ").Value(this.Message).Text(";"))</c>, or a plain string literal.
    /// </summary>
    public string ToWriterExpression(string text) {
        var parts = MarkerPattern.Split(text);
        if (parts.Length == 1) {
            return Literal(text);
        }

        var writer = $"__w{_writers++}";
        var expression = new StringBuilder($"global::Mumei.CodeGen.TemplateWriter.Build({writer} => {writer}");
        for (var i = 0; i < parts.Length; i++) {
            if (i % 2 == 0) {
                if (parts[i].Length > 0) {
                    expression.Append($".Text({Literal(parts[i])})");
                }

                continue;
            }

            var hole = _holes[int.Parse(parts[i])];
            expression.Append($".{hole.Kind}({hole.Expression})");
        }

        return expression.Append(')').ToString();
    }

    public bool IsGenerationTime(ISymbol? symbol) {
        if (symbol is null) {
            return false;
        }

        if (symbol is IParameterSymbol && SymbolEqualityComparer.Default.Equals(symbol, CallParameter)) {
            return true;
        }

        if (symbol is not (IPropertySymbol or IFieldSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })) {
            return false;
        }

        if (TemplateSymbols.IsTemplateBase(symbol.ContainingType)) {
            return true;
        }

        return SymbolEqualityComparer.Default.Equals(symbol.ContainingType?.OriginalDefinition, Template)
               && TemplateSymbols.IsGenerationTimeMember(symbol);
    }

    public static string Literal(string text) {
        return SymbolDisplay.FormatLiteral(text, quote: true);
    }
}

/// <summary>
/// Rewrites a template member into emitted code: generation-time expressions (template inputs, hooks, <c>call</c>, …)
/// are replaced with hole markers, everything else is globally qualified so it compiles in any namespace.
/// </summary>
internal sealed class StagingRewriter : GloballyQualifyingSyntaxRewriter {
    private static readonly SymbolDisplayFormat NonGenericQualifiedFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);

    private readonly TemplateStaging _staging;
    private readonly Dictionary<SyntaxNode, (RootKind Kind, string Expression)> _roots = new();
    private readonly HashSet<SyntaxNode> _droppedAttributes = [];

    private StagingRewriter(SemanticModel semanticModel, TemplateStaging staging, SyntaxNode scope) : base(semanticModel) {
        _staging = staging;
        FindRoots(scope);
    }

    private enum RootKind {
        Value,
        Interpolation,
        Code,
        Proceed,
        ReturnProceed
    }

    /// <summary>Stages <paramref name="node"/> and returns the rewritten node, containing hole markers.</summary>
    public static SyntaxNode Stage(SemanticModel semanticModel, TemplateStaging staging, SyntaxNode node, Func<AttributeSyntax, bool>? dropAttribute = null) {
        var rewriter = new StagingRewriter(semanticModel, staging, node);
        if (dropAttribute is not null) {
            foreach (var attribute in node.DescendantNodes().OfType<AttributeSyntax>().Where(dropAttribute)) {
                rewriter._droppedAttributes.Add(attribute);
            }
        }

        return rewriter.Visit(node)!;
    }

    public override SyntaxNode? Visit(SyntaxNode? node) {
        if (node is not null && _roots.TryGetValue(node, out var root)) {
            return ReplaceRoot(node, root.Kind, root.Expression);
        }

        return base.Visit(node);
    }

    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
        if (node.IsVar || node.IsNint || node.IsNuint || node.IsUnmanaged || node.IsNotNull) {
            return node;
        }

        return base.VisitIdentifierName(node);
    }

    public override SyntaxNode? VisitGenericName(GenericNameSyntax node) {
        // Qualify the name, but keep the type arguments so holes inside of them survive
        var symbol = SemanticModel.GetSymbolInfo(node).Symbol;
        if (symbol is not INamedTypeSymbol type || type.ContainingNamespace is null or { IsGlobalNamespace: true }) {
            return base.VisitGenericName(node);
        }

        if (node.Parent is QualifiedNameSyntax qualified && qualified.Right == node) {
            return base.VisitGenericName(node);
        }

        var typeArguments = (TypeArgumentListSyntax)Visit(node.TypeArgumentList)!;
        return GenericName(Identifier(type.ToDisplayString(NonGenericQualifiedFormat)), typeArguments)
            .WithTriviaFrom(node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node) {
        var visited = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node)!;
        if (node.Identifier.ValueText != _staging.Template.Name) {
            return visited;
        }

        var marker = _staging.AddHole(HoleKind.Code, "this.Name");
        return visited.WithIdentifier(Identifier(marker).WithTriviaFrom(visited.Identifier));
    }

    public override SyntaxNode? VisitAttributeList(AttributeListSyntax node) {
        if (!node.Attributes.Any(_droppedAttributes.Contains)) {
            return base.VisitAttributeList(node);
        }

        var remaining = node.Attributes.Where(a => !_droppedAttributes.Contains(a)).ToArray();
        if (remaining.Length == 0) {
            return null;
        }

        return base.VisitAttributeList(node.WithAttributes(SeparatedList(remaining)));
    }

    protected override bool ShouldQualify(IMethodSymbol target) {
        // Calls to other emitted members of the template stay unqualified, they're members of the generated type
        return !SymbolEqualityComparer.Default.Equals(target.ContainingType?.OriginalDefinition, _staging.Template);
    }

    private SyntaxNode ReplaceRoot(SyntaxNode node, RootKind kind, string expression) {
        switch (kind) {
            case RootKind.Interpolation: {
                var marker = _staging.AddHole(HoleKind.InterpolatedValue, expression);
                return InterpolatedStringText(Token(default, SyntaxKind.InterpolatedStringTextToken, marker, marker, default));
            }
            case RootKind.Proceed:
            case RootKind.ReturnProceed: {
                var invocation = node as InvocationExpressionSyntax ?? (InvocationExpressionSyntax)((ReturnStatementSyntax)node).Expression!;
                var arguments = invocation.ArgumentList.Arguments.Select(a => {
                    var staged = Visit(a.Expression)!.WithoutTrivia().ToFullString();
                    return _staging.ToWriterExpression(staged);
                });
                var method = kind == RootKind.Proceed ? "Proceed" : "ReturnProceed";
                var marker = _staging.AddHole(HoleKind.Code, $"__i.{method}({string.Join(", ", arguments)})");
                if (node is StatementSyntax) {
                    return ExpressionStatement(IdentifierName(marker), MissingToken(SyntaxKind.SemicolonToken)).WithTriviaFrom(node);
                }

                return IdentifierName(marker).WithTriviaFrom(node);
            }
            default: {
                var marker = _staging.AddHole(kind == RootKind.Code ? HoleKind.Code : HoleKind.Value, expression);
                return IdentifierName(marker).WithTriviaFrom(node);
            }
        }
    }

    private void FindRoots(SyntaxNode scope) {
        var opaque = new List<SyntaxNode>();
        foreach (var node in scope.DescendantNodesAndSelf()) {
            if (node is not SimpleNameSyntax name || opaque.Any(o => o.FullSpan.Contains(node.Span))) {
                continue;
            }

            var symbol = SemanticModel.GetSymbolInfo(name).Symbol;
            if (symbol is ITypeParameterSymbol typeParameter) {
                if (TryBindTypeParameter(typeParameter, name) is { } binding) {
                    AddRoot(name, RootKind.Value, binding);
                }

                continue;
            }

            if (symbol is INamedTypeSymbol type && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _staging.Template)) {
                AddRoot(name, RootKind.Code, "this.Name");
                continue;
            }

            if (!_staging.IsGenerationTime(symbol)) {
                continue;
            }

            ExpressionSyntax start = name;
            if (name.Parent is MemberAccessExpressionSyntax access && access.Name == name) {
                if (access.Expression is not (ThisExpressionSyntax or BaseExpressionSyntax)) {
                    continue;
                }

                start = access;
            } else if (name.Parent is MemberBindingExpressionSyntax or NameColonSyntax or NameEqualsSyntax) {
                continue;
            }

            var root = Climb(start);
            if (IsCallMethod(root, "Proceed")) {
                var invocation = (InvocationExpressionSyntax)root;
                if (invocation.Parent is ReturnStatementSyntax ret) {
                    _roots[ret] = (RootKind.ReturnProceed, "");
                } else if (invocation.Parent is ArrowExpressionClauseSyntax arrow
                           && arrow.Parent is MethodDeclarationSyntax method
                           && SymbolEqualityComparer.Default.Equals(SemanticModel.GetDeclaredSymbol(method), _staging.InterceptMethod)) {
                    _roots[invocation] = (RootKind.ReturnProceed, "");
                } else {
                    _roots[invocation] = (RootKind.Proceed, "");
                }

                // The arguments are staged when replacing the root, so we keep looking for roots inside of them
                continue;
            }

            if (IsCallMethod(root, "Arg")) {
                var argument = ((InvocationExpressionSyntax)root).ArgumentList.Arguments.FirstOrDefault()?.Expression;
                AddRoot(root, RootKind.Value, $"__i.Arg({argument})");
                continue;
            }

            if (root.Parent is InterpolationSyntax { AlignmentClause: null, FormatClause: null } interpolation
                && interpolation.Parent is InterpolatedStringExpressionSyntax interpolated
                && interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedStringStartToken)) {
                CheckStaging(root);
                AddRoot(interpolation, RootKind.Interpolation, root.ToString());
                continue;
            }

            CheckStaging(root);
            AddRoot(root, RootKind.Value, root.ToString());
        }

        void AddRoot(SyntaxNode root, RootKind kind, string expression) {
            _roots[root] = (kind, expression);
            opaque.Add(root);
        }
    }

    private string? TryBindTypeParameter(ITypeParameterSymbol typeParameter, SyntaxNode usage) {
        if (SymbolEqualityComparer.Default.Equals(typeParameter.DeclaringType?.OriginalDefinition, _staging.Template)) {
            if (_staging.TypeParameterBindings.TryGetValue(typeParameter, out var binding)) {
                return binding;
            }

            // Reported once per template by the emitter
            return null;
        }

        if (_staging.InterceptMethod is { } intercept
            && SymbolEqualityComparer.Default.Equals(typeParameter.DeclaringMethod?.OriginalDefinition, intercept.OriginalDefinition)) {
            return $"{_staging.CallParameter!.Name}.Method.ReturnType";
        }

        return null;
    }

    private ExpressionSyntax Climb(ExpressionSyntax start) {
        var current = start;
        while (!IsCallMethod(current, "Proceed") && !IsCallMethod(current, "Arg")) {
            switch (current.Parent) {
                case MemberAccessExpressionSyntax access when access.Expression == current:
                    current = access;
                    continue;
                case InvocationExpressionSyntax invocation when invocation.Expression == current:
                    current = invocation;
                    continue;
                case ElementAccessExpressionSyntax element when element.Expression == current:
                    current = element;
                    continue;
                case ConditionalAccessExpressionSyntax conditional when conditional.Expression == current:
                    current = conditional;
                    continue;
                case PostfixUnaryExpressionSyntax postfix when postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression):
                    current = postfix;
                    continue;
            }

            break;
        }

        return current;
    }

    private bool IsCallMethod(ExpressionSyntax expression, string name) {
        return expression is InvocationExpressionSyntax
               && SemanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol method
               && method.Name == name
               && TemplateSymbols.IsMumeiType(method.ContainingType, "Call");
    }

    private void CheckStaging(SyntaxNode root) {
        foreach (var identifier in root.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()) {
            var symbol = SemanticModel.GetSymbolInfo(identifier).Symbol;
            if (symbol is not (ILocalSymbol or IParameterSymbol or IRangeVariableSymbol)
                || SymbolEqualityComparer.Default.Equals(symbol, _staging.CallParameter)) {
                continue;
            }

            var declaredInside = symbol.DeclaringSyntaxReferences.Any(r => r.SyntaxTree == root.SyntaxTree && root.Span.Contains(r.Span));
            if (!declaredInside) {
                _staging.Diagnostics.Add(DiagnosticInfo.Create(
                    TemplateDiagnostics.RuntimeValueInGenerationTimeExpression,
                    identifier,
                    identifier.Identifier.ValueText,
                    root.ToString()
                ));
            }
        }
    }
}

internal static class TemplateSymbols {
    public static bool IsMumeiType(INamedTypeSymbol? type, string name) {
        return type is not null
               && type.Name == name
               && type.ContainingNamespace?.ToDisplayString() == "Mumei.CodeGen"
               && type.ContainingType is null;
    }

    public static bool IsTemplateBase(INamedTypeSymbol? type) {
        return IsMumeiType(type, "Template") || IsMumeiType(type, "ClassTemplate") || IsMumeiType(type, "InterceptorTemplate");
    }

    public static bool HasAttribute(ISymbol symbol, string name) {
        return symbol.GetAttributes().Any(a => IsMumeiType(a.AttributeClass, name));
    }

    /// <summary>Inputs, hook overrides and [CompileTime] members run while generating and are not emitted.</summary>
    public static bool IsGenerationTimeMember(ISymbol symbol) {
        if (symbol is IPropertySymbol { IsRequired: true } || HasAttribute(symbol, "CompileTimeAttribute")) {
            return true;
        }

        var overridden = symbol switch {
            IPropertySymbol p => (ISymbol?)p.OverriddenProperty,
            IMethodSymbol m => m.OverriddenMethod,
            _ => null
        };
        while (overridden is not null) {
            if (IsTemplateBase(overridden.ContainingType)) {
                return true;
            }

            overridden = overridden switch {
                IPropertySymbol p => p.OverriddenProperty,
                IMethodSymbol m => m.OverriddenMethod,
                _ => null
            };
        }

        return false;
    }
}
