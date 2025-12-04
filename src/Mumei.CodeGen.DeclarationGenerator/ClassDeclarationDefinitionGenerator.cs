using System.Collections.Immutable;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                    "φbuilder",
                    ctx.TypeFromCompilation(typeof(ISyntheticClassBuilder<>)).Construct(classDefinitionSymbol),
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
            if (property.Type is ITypeParameterSymbol propertyTypeParam) {
                renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.DynamicallyBoundType)}(nameof({propertyTypeParam.Name}))");
            } else {
                renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context.Type)}({type.TypeOf})");
            }

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
            if (field.Type is ITypeParameterSymbol propertyTypeParam) {
                renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.DynamicallyBoundType)}(nameof({propertyTypeParam.Name}))");
            } else {
                renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context.Type)}({type.TypeOf})");
            }
            renderTree.Line(",");
            renderTree.InterpolatedLine($"{field.Name:q}");
            renderTree.EndBlock();
            renderTree.Line(");");
        };
    }
}