using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.Components;

public static class MethodDeclarationRoslynExtensions {
    extension<TSignature>(ISyntheticInterceptorMethodBuilder<TSignature> method) where TSignature : Delegate {
        public ISyntheticMethod<TSignature> WithReturnType(ITypeSymbol typeSymbol) {
            return method.WithReturnType(method.ΦCompilerApi.Context.Type(typeSymbol));
        }
    }

    extension(SyntheticMethodDefinition.BindingContext ctx) {
        public void Bind<T>(ITypeSymbol typeSymbol) {
            ctx.Bind<T>(Type.GetType(typeSymbol.ToDisplayString())!);
        }
    }

    extension(SyntheticInterceptorMethodDefinition.BindingContext ctx) {
        public void Bind<T>(ITypeSymbol typeSymbol) {
            ctx.Bind<T>(Type.GetType(typeSymbol.ToDisplayString())!);
        }
    }
}