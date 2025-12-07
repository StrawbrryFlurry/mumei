using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.Components;

public static class MethodDeclarationRoslynExtensions {
    extension<TSignature>(ISyntheticInterceptorMethodBuilder<TSignature> method) where TSignature : Delegate {
        public ISyntheticMethod<TSignature> WithReturnType(ITypeSymbol typeSymbol) {
            return method.WithReturnType(method.ΦCompilerApi.Context.Type(typeSymbol));
        }
    }

    extension(MethodDefinitionBindingContext ctx) {
        public void Bind(Type targetType, ITypeSymbol typeSymbol, [CallerArgumentExpression(nameof(targetType))] string targetTypeExpression = "") {
            ctx.Bind(targetType, new RoslynSyntheticType(typeSymbol), targetTypeExpression);
        }
    }
}