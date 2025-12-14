using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.Components;

public static class SyntheticMethodBuilderExtensions {
    extension<TSignature>(ISyntheticMethodBuilder<TSignature> builder) where TSignature : Delegate {
        public ISyntheticMethodBuilder<TSignature> UpdateSignatureFrom(IMethodSymbol target) {
            var ctx = builder.ΦCompilerApi.Context;
            ApplyMethodSignatureToBuilder(ctx, builder, target);
            return builder;
        }

        public ISyntheticMethodBuilder<TSignature> WithReturnType(ITypeSymbol returnType) {
            builder.WithReturnType(builder.ΦCompilerApi.Context.Type(returnType));
            return builder;
        }
    }

    private static void ApplyMethodSignatureToBuilder<TSignature>(
        ICodeGenerationContext ctx,
        ISyntheticMethodBuilder<TSignature> builder,
        IMethodSymbol method
    ) where TSignature : Delegate {
        var parameterList = ctx.ParameterListFrom(method);
        builder.WithParameters(parameterList);

        var typeParameters = ctx.TypeParameterListFrom(method);
        builder.WithTypeParameters(typeParameters);

        var returnType = ctx.Type(method.ReturnType);
        builder.WithReturnType(returnType);
    }

}