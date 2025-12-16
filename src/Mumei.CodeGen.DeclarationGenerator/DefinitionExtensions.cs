using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.DeclarationGenerator;

public static class DefinitionExtensions {
    extension(ISimpleSyntheticClassBuilder classBuilder) {
        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod<TMethodDefinition>(
            SyntheticIdentifier name,
            InvocationExpressionSyntax invocationToIntercept,
            Func<TMethodDefinition, Delegate> methodSelector,
            Action<TMethodDefinition>? inputBinder = null
        ) where TMethodDefinition : SyntheticInterceptorMethodDefinition, new() {
            throw new NotImplementedException();
        }
    }
}