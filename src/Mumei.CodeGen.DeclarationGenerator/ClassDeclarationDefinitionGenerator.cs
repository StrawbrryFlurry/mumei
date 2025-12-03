using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.CodeGen.Roslyn.RoslynCodeProviders;

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
            .WithAccessibility(classDefinitionSymbol.DeclaredAccessibility.ToAccessModifiers() + AccessModifier.Partial);

        ctx.EmitUnique(classDefinitionSymbol.Name, definitionCodeGenClass);
    }
}