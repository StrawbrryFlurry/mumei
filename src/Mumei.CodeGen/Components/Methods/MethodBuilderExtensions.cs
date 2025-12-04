namespace Mumei.CodeGen.Components;

public static class MethodBuilderExtensions {
    extension<TSignature>(ISyntheticMethodBuilder<TSignature> builder) where TSignature : Delegate {
        public ISyntheticMethodBuilder<TSignature> WithReturnType(Type returnType) {
            var ctx = builder.ΦCompilerApi.Context;
            builder.WithReturnType(ctx.Type(returnType));
            return builder;
        }
    }
}