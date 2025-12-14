using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.DeclarationGenerator;

public sealed partial class DeclarationDefinitionGenerator {
    public static void EmitBindCompilerOutputMembersMethodForClass(
        ICodeGenerationContext ctx,
        ISimpleSyntheticClassBuilder definitionCodeGenClass,
        INamedTypeSymbol definitionType
    ) {
        var bindingMethod = definitionCodeGenClass.DeclareMethod<Action<ISyntheticClassBuilder<CompileTimeUnknown>>>(
                nameof(SyntheticClassDefinition<>.InternalBindCompilerOutputMembers)
            )
            .WithAccessibility(AccessModifier.Public + AccessModifier.Override)
            .WithReturnType(typeof(void))
            .WithParameters(
                ctx.Parameter(
                    ctx.TypeFromCompilation(typeof(ISyntheticClassBuilder<>)).Construct(definitionType),
                    $"{Strings.PrivateLocal}builder",
                    out var classBuilder
                )
            );

        var inputMemberNames = new ArrayBuilder<string>();
        var outputMembers = new ArrayBuilder<ISymbol>();

        CollectOutputAndInputMembers(ctx, definitionType, ref inputMemberNames, ref outputMembers);

        var memberBindingStatements = new ArrayBuilder<StatementBuilder>(outputMembers.Count);

        foreach (var member in outputMembers) {
            if (member is IFieldSymbol field) {
                memberBindingStatements.Add(DeclarationBuilderFactory.DeclareFieldFromDefinitionMember(classBuilder, field));
                continue;
            }

            if (member is IPropertySymbol property) {
                memberBindingStatements.Add(DeclarationBuilderFactory.DeclarePropertyFromDefinitionMember(classBuilder, property));
                continue;
            }

            if (member is IMethodSymbol method) {
                var sm = ctx.Compilation.GetSemanticModel(method.DeclaringSyntaxReferences[0].SyntaxTree);
                var bindingExpression = DeclarationBuilderFactory.DeclareMethodFromDefinition(method, classBuilder, sm, inputMemberNames);
                memberBindingStatements.Add(bindingExpression);
            }
        }

        inputMemberNames.Dispose();
        outputMembers.Dispose();

        bindingMethod.WithBody(ctx.Block(memberBindingStatements.ToArrayAndFree(), static (renderTree, bindingExpressions) => {
            for (var i = 0; i < bindingExpressions.Length; i++) {
                var bindingExpression = bindingExpressions[i];
                renderTree.Node(bindingExpression);
                if (i < bindingExpressions.Length - 1) {
                    renderTree.NewLine();
                }
            }
        }));
    }

}