using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Roslyn.RoslynCodeProviders;

namespace Mumei.CodeGen.DeclarationGenerator;

[Generator]
public sealed partial class DeclarationDefinitionGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var declareDefinitionInvocations = context.CreateQtProvider(
            (node, token) => {
                if (
                    SyntaxNodeFilter.IsClassDeclarationImplementing(
                        node,
                        nameof(SyntheticClassDefinition<>),
                        out var cls,
                        out var baseType
                    )
                ) {
                    if (baseType is GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 }) {
                        return true;
                    }
                }

                if (
                    SyntaxNodeFilter.IsClassDeclarationImplementing(
                        node,
                        nameof(SyntheticMethodDefinition),
                        out cls,
                        out _
                    )
                ) {
                    return true;
                }

                if (
                    SyntaxNodeFilter.IsClassDeclarationImplementing(
                        node,
                        nameof(SyntheticInterceptorMethodDefinition),
                        out cls,
                        out _
                    )
                ) {
                    return true;
                }

                return false;
            },
            (syntaxContext, ct) => {
                var node = syntaxContext.Node;
                if (node is not ClassDeclarationSyntax classNode) {
                    return default;
                }

                if (syntaxContext.SemanticModel.GetDeclaredSymbol(classNode, ct) is not { } classType) {
                    return default;
                }

                if (classType.BaseType!.MetadataName is var baseName
                    && baseName != typeof(SyntheticClassDefinition<>).Name
                    && baseName != nameof(SyntheticMethodDefinition)
                    && baseName != nameof(SyntheticInterceptorMethodDefinition)
                   ) {
                    return default;
                }

                return (classNode, classType);
            }
        );

        var output = declareDefinitionInvocations.IncrementalGenerate(GenerateCode);

        context.RegisterCodeGenerationOutput(output);
    }
}