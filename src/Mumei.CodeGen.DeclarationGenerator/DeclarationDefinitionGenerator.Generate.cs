using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Roslyn.Components;
using Eq = Microsoft.CodeAnalysis.SymbolEqualityComparer;

namespace Mumei.CodeGen.DeclarationGenerator;

public sealed partial class DeclarationDefinitionGenerator {
    private static void GenerateCode(ICodeGenerationContext ctx, (ClassDeclarationSyntax DefinitionDeclaration, INamedTypeSymbol DefinitionType) inputs) {
        var syntheticClassDefinitionType = ctx.TypeFromCompilation(typeof(SyntheticClassDefinition<>));
        var syntheticMethodDefinitionType = ctx.TypeFromCompilation(typeof(SyntheticMethodDefinition));
        var syntheticInterceptorMethodDefinitionType = ctx.TypeFromCompilation(typeof(SyntheticInterceptorMethodDefinition));

        var (definitionDeclaration, definitionType) = inputs;
        var ns = ctx.Namespace(definitionType.ContainingNamespace);

        if (!definitionDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)) {
            throw new NotSupportedException();
        }

        var definitionCodeGenClass = ns.DeclareClass(definitionType.Name)
            .WithTypeParametersFrom(definitionType)
            .WithAccessibility(definitionType.DeclaredAccessibility.ToAccessModifiers() + AccessModifier.Partial);

        if (Eq.Default.Equals(definitionType.BaseType, syntheticClassDefinitionType.Construct(definitionType))) {
            EmitBindCompilerOutputMembersMethodForClass(ctx, definitionCodeGenClass, definitionDeclaration, definitionType);
        } else if (Eq.Default.Equals(definitionType.BaseType, syntheticMethodDefinitionType)) {
            EmitInternalBindCompilerMethodForMethod(ctx, definitionCodeGenClass, definitionDeclaration, definitionType);
        } else if (Eq.Default.Equals(definitionType.BaseType, syntheticInterceptorMethodDefinitionType)) {
            EmitInternalBindCompilerMethodForInterceptorMethod(ctx, definitionCodeGenClass, definitionType);
        }

        ctx.EmitUnique(definitionType.Name, definitionCodeGenClass);
    }

    public static void EmitInternalBindCompilerMethodForInterceptorMethod(
        ICodeGenerationContext ctx,
        ISimpleSyntheticClassBuilder definitionCodeGenClass,
        INamedTypeSymbol definitionType
    ) { }
}