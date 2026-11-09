using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Legacy.Components;

namespace Mumei.CodeGen.Legacy.Roslyn.Components;

public static class MethodDeclarationRoslynExtensions {
    extension<TSignature>(ISyntheticInterceptorMethodBuilder<TSignature> method) where TSignature : Delegate {
        public ISyntheticMethod<TSignature> WithReturnType(ITypeSymbol typeSymbol) {
            return method.WithReturnType(method.ΦCompilerApi.Context.Type(typeSymbol));
        }
    }

    extension(SyntheticDeclarationDefinition declaration) {
        public void Bind(Type targetType, ITypeSymbol typeSymbol,
            [CallerArgumentExpression(nameof(targetType))] string targetTypeExpression = "") {
            declaration.Bind(targetType, declaration.CodeGenContext.Type(typeSymbol), targetTypeExpression);
        }
    }
}