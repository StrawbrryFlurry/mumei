using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.CodeGen.Roslyn.RoslynCodeProviders;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.DeclarationGenerator;

[Generator]
public sealed class ClassDeclarationDefinitionGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var declareClassDefinitionInvocations = context.CreateQtProvider(
            (node, token) => {
                if (
                    !SyntaxNodeFilter.IsClassDeclarationImplementing(
                        node,
                        nameof(SyntheticClassDefinition<>),
                        out var cls,
                        out var baseType
                    )
                ) {
                    return false;
                }

                if (baseType is not GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 }) {
                    return false;
                }

                return true;
            },
            (syntaxContext, ct) => {
                var node = syntaxContext.Node;
                if (node is not ClassDeclarationSyntax classNode) {
                    return default;
                }

                if (syntaxContext.SemanticModel.GetDeclaredSymbol(classNode, ct) is not { } classType) {
                    return default;
                }

                if (classType.BaseType?.MetadataName != typeof(SyntheticClassDefinition<>).Name) {
                    return default;
                }

                return (classNode, classType);
            }
        );

        var output = declareClassDefinitionInvocations.IncrementalGenerate(GenerateCode);

        context.RegisterCodeGenerationOutput(output);
    }

    private static void GenerateCode(ICodeGenerationContext ctx, (ClassDeclarationSyntax ClassDefinition, INamedTypeSymbol ClassDefinitionSymbol) inputs) {
        var classDefinitionSyntax = inputs.ClassDefinition;
        var classDefinitionSymbol = inputs.ClassDefinitionSymbol;
        var ns = ctx.Namespace(classDefinitionSymbol.ContainingNamespace);

        if (!classDefinitionSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)) {
            throw new NotSupportedException();
        }

        var definitionCodeGenClass = ns.DeclareClass(classDefinitionSymbol.Name)
            .WithTypeParametersFrom(classDefinitionSymbol)
            .WithAccessibility(classDefinitionSymbol.DeclaredAccessibility.ToAccessModifiers() + AccessModifier.Partial);

        var bindingMethod = definitionCodeGenClass.DeclareMethod<Action<ISyntheticClassBuilder<CompileTimeUnknown>>>(
                nameof(SyntheticClassDefinition<>.InternalBindCompilerOutputMembers)
            )
            .WithAccessibility(AccessModifier.Public + AccessModifier.Override)
            .WithReturnType(typeof(void))
            .WithParameters(
                ctx.Parameter(
                    ctx.TypeFromCompilation(typeof(ISyntheticClassBuilder<>)).Construct(classDefinitionSymbol),
                    $"{Strings.PrivateLocal}builder",
                    out var builderParameter
                )
            );

        var bindingExpressions = CollectOutputMemberBindingExpressions(ctx, classDefinitionSymbol, builderParameter);
        bindingMethod.WithBody(ctx.Block(bindingExpressions, static (renderTree, bindingExpressions) => {
            for (var i = 0; i < bindingExpressions.Length; i++) {
                var bindingExpression = bindingExpressions[i];
                renderTree.Node(bindingExpression);
                if (i < bindingExpressions.Length - 1) {
                    renderTree.NewLine();
                }
            }
        }));

        ctx.EmitUnique(classDefinitionSymbol.Name, definitionCodeGenClass);
    }

    private static ImmutableArray<RenderFragment> CollectOutputMemberBindingExpressions(
        ICodeGenerationContext ctx,
        INamedTypeSymbol classDefinitionSymbol,
        ExpressionFragment classBuilder
    ) {
        var bindingExpressions = new ArrayBuilder<RenderFragment>();
        var outputAttribute = ctx.TypeFromCompilation<OutputAttribute>();
        var inputAttribute = ctx.TypeFromCompilation<InputAttribute>();

        var inputMemberNames = classDefinitionSymbol.GetMembers()
            .Where(m => m.HasAttribute(inputAttribute))
            .Select(m => m.Name);
        var inputMembers = new HashSet<string>(inputMemberNames);

        foreach (var member in classDefinitionSymbol.GetMembers()) {
            if (!member.HasAttribute(outputAttribute)) {
                continue;
            }

            if (member is IFieldSymbol field) {
                var bindingExpression = MakeFieldOutputMemberBindingExpression(field, classBuilder);
                bindingExpressions.Add(bindingExpression);
                continue;
            }

            if (member is IPropertySymbol property) {
                var bindingExpression = MakePropertyOutputMemberBindingExpression(property, classBuilder);
                bindingExpressions.Add(bindingExpression);
                continue;
            }

            if (member is IMethodSymbol method) {
                var sm = ctx.Compilation.GetSemanticModel(method.DeclaringSyntaxReferences[0].SyntaxTree);
                var bindingExpression = MakeMethodOutputMemberBindingExpression(method, classBuilder, sm, inputMembers);
                bindingExpressions.Add(bindingExpression);
                continue;
            }
        }

        return bindingExpressions.ToImmutableArrayAndFree();
    }


    private static RenderFragment MakePropertyOutputMemberBindingExpression(
        IPropertySymbol property,
        ExpressionFragment classBuilder
    ) {
        var type = property.Type is ITypeParameterSymbol ? new TypeInfoFragment(typeof(CompileTimeUnknown)) : property.Type.ToRenderFragment();

        return renderTree => {
            renderTree.InterpolatedLine($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.DeclareProperty)}<{type.FullName}>(");
            renderTree.StartBlock();
            WritePossiblyLateBoundGetTypeExpression(property.Type, classBuilder, renderTree);
            renderTree.Line(",");
            renderTree.InterpolatedLine($"{property.Name:q},");
            renderTree.InterpolatedLine($"new {typeof(SyntheticPropertyAccessorList)}(");
            renderTree.StartBlock();
            // ToDo: Create these based of the declared syntax of the property.
            renderTree.InterpolatedLine($"{typeof(SyntheticPropertyAccessor)}.{nameof(SyntheticPropertyAccessor.SimpleGetter)},");
            renderTree.Line("null");
            renderTree.EndBlock();
            renderTree.Line(")");
            renderTree.EndBlock();
            renderTree.Line(");");
        };
    }

    private static RenderFragment MakeFieldOutputMemberBindingExpression(
        IFieldSymbol field,
        ExpressionFragment classBuilder
    ) {
        var type = field.Type is ITypeParameterSymbol ? new TypeInfoFragment(typeof(CompileTimeUnknown)) : field.Type.ToRenderFragment();

        return renderTree => {
            renderTree.InterpolatedLine($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.DeclareField)}<{type.FullName}>(");
            renderTree.StartBlock();
            WritePossiblyLateBoundGetTypeExpression(field.Type, classBuilder, renderTree);
            renderTree.Line(",");
            renderTree.InterpolatedLine($"{field.Name:q}");
            renderTree.EndBlock();
            renderTree.Line(");");
        };
    }

    private static RenderFragment MakeMethodOutputMemberBindingExpression(
        IMethodSymbol method,
        ExpressionFragment classBuilder,
        SemanticModel sm,
        HashSet<string> inputMemberNames
    ) {
        if (method.DeclaringSyntaxReferences is not [var declarationRef]) {
            throw new NotSupportedException();
        }

        if (declarationRef.GetSyntax() is not MethodDeclarationSyntax methodDeclarationSyntax) {
            throw new NotSupportedException();
        }

        var accessibility = methodDeclarationSyntax.Modifiers.ToAccessModifiers();

        return renderTree => {
            renderTree.Interpolate($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.DeclareMethod)}");
            renderTree.Interpolate($"<global::System.Delegate>");
            renderTree.InterpolatedLine($"({method.Name:q})");
            renderTree.StartBlock();

            renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithAccessibility)}({accessibility.RenderAsExpression})");
            renderTree.Interpolate($".{nameof(ISyntheticMethodBuilder<>.WithReturnType)}(");
            WritePossiblyLateBoundGetTypeExpression(method.ReturnType, classBuilder, renderTree);
            renderTree.Line(")");

            if (method.Parameters.Length != 0) {
                renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithParameters)}(");
                renderTree.StartBlock();
                for (var i = 0; i < method.Parameters.Length; i++) {
                    var methodParameter = method.Parameters[i];
                    renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ICodeGenerationContext.Parameter)}(");
                    renderTree.StartBlock();
                    WritePossiblyLateBoundGetTypeExpression(methodParameter.Type, classBuilder, renderTree);
                    renderTree.Line(",");
                    renderTree.InterpolatedLine($"{methodParameter.Name:q}");
                    renderTree.EndBlock();
                    renderTree.Line(")");
                    if (i < method.Parameters.Length - 1) {
                        renderTree.Line(",");
                    }
                }

                renderTree.EndBlock();
                renderTree.Line(")");
            }

            if (method.TypeParameters.Length != 0) {
                renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithTypeParameters)}(");
                renderTree.StartBlock();

                for (var i = 0; i < method.TypeParameters.Length; i++) {
                    var typeParameter = method.TypeParameters[i];
                    renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ICodeGenerationContext.TypeParameter)}(");
                    renderTree.StartBlock();
                    renderTree.InterpolatedLine($"{typeParameter.Name:q}");
                    renderTree.EndBlock();
                    renderTree.Text(")");
                    if (i < method.Parameters.Length - 1) {
                        renderTree.Line(",");
                    }
                }

                renderTree.EndBlock();
                renderTree.Line(")");
            }

            if (method.IsAbstract) {
                renderTree.Line(";");
                renderTree.EndBlock();
                return;
            }

            renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithBody)}(");
            renderTree.StartBlock();
            renderTree.InterpolatedLine($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ICodeGenerationContext.Block)}(");
            renderTree.StartBlock();
            renderTree.Line("this,");
            renderTree.InterpolatedLine($"static ({ClassDefinitionOutputMethodCodeBlockResolutionContext.RenderTreeParameter}, {ClassDefinitionOutputMethodCodeBlockResolutionContext.InputArgumentName}) => {{");
            renderTree.StartBlock();
            Debug.Assert(methodDeclarationSyntax.Body is not null);
            var renderBody = SyntheticRenderCodeBlockSyntaxRewriter.CreateSyntheticRenderBlock(methodDeclarationSyntax.Body, method.ReturnType, sm, new ClassDefinitionOutputMethodCodeBlockResolutionContext(sm, inputMemberNames));
            renderTree.Block(renderBody);
            renderTree.NewLine();
            renderTree.EndBlock();
            renderTree.Line("}");
            renderTree.EndBlock();
            renderTree.Line(")");
            renderTree.EndBlock();
            renderTree.Line(");");
            renderTree.EndBlock();
        };
    }

    public static void WritePossiblyLateBoundGetTypeExpression(
        ITypeSymbol type,
        ExpressionFragment classBuilder,
        IRenderTreeBuilder renderTreeBuilder
    ) {
        if (type is not ITypeParameterSymbol { DeclaringMethod: null } typeParameter) {
            renderTreeBuilder.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context.Type)}({type.ToRenderFragment().TypeOf})");
            return;
        }

        renderTreeBuilder.Interpolate($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.DynamicallyBoundType)}(nameof({typeParameter.Name}))");
    }
}

internal sealed class ClassDefinitionOutputMethodCodeBlockResolutionContext(
    SemanticModel sm,
    HashSet<string> inputMemberNames
) : ISyntheticCodeBlockResolutionContext {
    public const string RenderTreeParameter = $"{Strings.PrivateLocal}renderTree";
    public const string InputArgumentName = $"{Strings.PrivateLocal}ctx";

    public MacroRenderTreeBuilder MacroRenderContext { get; set; }

    public bool TryResolveThisExpression(ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        // If this is not a member access to an input member we don't care about it since `this` will be valid in the generated context.
        slot = null;
        return false;
    }

    public bool TryUpdateThisExpression(ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveName(SimpleNameSyntax identifier, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        if (!inputMemberNames.Contains(identifier.Identifier.Text)) {
            goto NotFound;
        }

        var memberOperation = sm.GetOperation(identifier);
        if (memberOperation is not IMemberReferenceOperation memberReference) {
            goto NotFound;
        }

        // Replace Member with ctx.Member
        var synthesizedMemberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(InputArgumentName),
            identifier
        );
        EmitInputMemberRenderExpression(identifier, synthesizedMemberAccess, memberReference.Member, out slot);
        return true;

        NotFound:
        slot = null;
        return false;
    }

    public bool TryUpdateName(SimpleNameSyntax identifier, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveMemberAccess(MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        var targetMemberAccess = GetInnerMostMemberAccess(memberAccess);
        if (targetMemberAccess.Expression is not ThisExpressionSyntax || !inputMemberNames.Contains(targetMemberAccess.Name.Identifier.Text)) {
            goto NotFound;
        }

        var memberOperation = sm.GetOperation(memberAccess);
        if (memberOperation is not IMemberReferenceOperation memberReference) {
            goto NotFound;
        }

        // Replace this.Member with ctx.Member
        var synthesizedMemberAccess = memberAccess.ReplaceNode(targetMemberAccess.Expression, SyntaxFactory.IdentifierName(InputArgumentName));
        EmitInputMemberRenderExpression(memberAccess, synthesizedMemberAccess, memberReference.Member, out slot);
        return true;

        NotFound:
        slot = null;
        return false;
    }

    private void EmitInputMemberRenderExpression(
        ExpressionSyntax memberAccess,
        ExpressionSyntax synthesizedMemberAccess,
        ISymbol memberSymbol,
        out MacroRenderTreeBuilder.MacroSlot slot
    ) {
        ITypeSymbol memberType;
        if (memberSymbol is IPropertySymbol propertySymbol) {
            memberType = propertySymbol.Type;
        } else if (memberSymbol is IFieldSymbol fieldSymbol) {
            memberType = fieldSymbol.Type;
        } else if (memberSymbol is IMethodSymbol methodSymbol) {
            memberType = methodSymbol.ReturnType;
        } else {
            slot = MacroRenderContext.DeclareSlot(renderTree => {
                renderTree.Text($"#error Failed to generate a render expression for {memberAccess} since {memberSymbol} is not a valid renderable member.");
            });
            return;
        }

        if (TryEmitRenderExpressionForType(memberAccess, synthesizedMemberAccess, memberType, out slot)) {
            return;
        }

        slot = MacroRenderContext.DeclareSlot(renderTree => {
            renderTree.Text(
                "#warning Failed to generate a render expression for {memberAccess} since it's type '{memberType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}' can only be used in synthetic code blocks if it implements '{nameof(IRenderFragment)}', or if it has a suitable '{nameof(DefaultRenderExpressionExtensions.RenderTo)}' method that takes a {nameof(IRenderTreeBuilder)} as its only parameter."
            );
        });
    }

    private bool TryEmitRenderExpressionForType(
        ExpressionSyntax expressionToRender,
        ExpressionSyntax synthesizedExpressionToRender,
        ITypeSymbol typeSymbol,
        [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot
    ) {
        var renderFragmentType = sm.Compilation.GetTypeByMetadataName(typeof(IRenderFragment).FullName!)!;
        if (typeSymbol.AllInterfaces.Contains(renderFragmentType, SymbolEqualityComparer.Default)) {
            slot = MacroRenderContext.DeclareSlot(renderTree => {
                renderTree.Interpolate($"{synthesizedExpressionToRender.NormalizeWhitespace().ToFullString()}.{nameof(IRenderFragment.Render)}({RenderTreeExpression()});");
            });
            return true;
        }

        var syntheticRenderInvocationExpression = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                expressionToRender,
                SyntaxFactory.IdentifierName(nameof(DefaultRenderExpressionExtensions.RenderExpression))
            ),
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(
                        SyntaxFactory.DefaultExpression(SyntaxFactory.IdentifierName(RuntimeTypeSerializer.GetTypeFullName(typeof(IRenderTreeBuilder)))
                        )
                    )
                )
            )
        );

        var syntheticRenderInvocationInformation = sm.GetSpeculativeSymbolInfo(
            expressionToRender.SpanStart,
            syntheticRenderInvocationExpression,
            SpeculativeBindingOption.BindAsExpression
        );

        if (syntheticRenderInvocationInformation.CandidateSymbols is not [IMethodSymbol renderToMethodSymbol]) {
            slot = null;
            return false;
        }

        if (!renderToMethodSymbol.IsExtensionMethod) {
            slot = MacroRenderContext.DeclareSlot(renderTree => {
                renderTree.Interpolate($"{synthesizedExpressionToRender.NormalizeWhitespace().ToFullString()}.{renderToMethodSymbol.Name}({RenderTreeExpression()});");
            });
            return true;
        }

        slot = MacroRenderContext.DeclareSlot(renderTree => {
            renderTree.Interpolate($"{renderToMethodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{renderToMethodSymbol.Name}({synthesizedExpressionToRender.NormalizeWhitespace().ToFullString()}, {RenderTreeExpression()});");
        });
        return true;
    }

    private static MemberAccessExpressionSyntax GetInnerMostMemberAccess(MemberAccessExpressionSyntax memberAccess) {
        var innerMostMemberAccess = memberAccess;
        var nextExpression = memberAccess.Expression;
        while (nextExpression is MemberAccessExpressionSyntax innerMemberAccess) {
            nextExpression = innerMemberAccess.Expression;
            innerMostMemberAccess = innerMemberAccess;
        }

        return innerMostMemberAccess;
    }

    public bool TryUpdateMemberAccess(MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveInvocation(InvocationExpressionSyntax invocation, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        ExpressionSyntax synthesizedInvocationExpression;
        if (invocation.Expression is not MemberAccessExpressionSyntax && invocation.Expression is not SimpleNameSyntax) {
            goto NotFound;
        }

        if (invocation.Expression is MemberAccessExpressionSyntax potentialInputAccess) {
            var memberAccess = GetInnerMostMemberAccess(potentialInputAccess);
            if (memberAccess.Expression is not ThisExpressionSyntax || !inputMemberNames.Contains(memberAccess.Name.Identifier.Text)) {
                goto NotFound;
            }

            synthesizedInvocationExpression = invocation.ReplaceNode(memberAccess.Expression, SyntaxFactory.IdentifierName(InputArgumentName));
        } else {
            var invocationTarget = (SimpleNameSyntax) invocation.Expression;
            synthesizedInvocationExpression = invocation.WithExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(InputArgumentName),
                    invocationTarget
                )
            );
        }

        if (sm.GetOperation(invocation) is not IInvocationOperation invocationOperation) {
            goto NotFound;
        }

        EmitInputMemberRenderExpression(invocation, synthesizedInvocationExpression, invocationOperation.TargetMethod, out slot);
        return true;

        NotFound:
        slot = null;
        return false;
    }

    public bool TryUpdateInvocation(InvocationExpressionSyntax invocation, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string RenderTreeExpression() {
        return RenderTreeParameter;
    }
}