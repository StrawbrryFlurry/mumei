using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Roslyn.Components;

namespace Mumei.CodeGen.DeclarationGenerator;

public static class DefinitionExtensions {
    extension(ISimpleSyntheticClassBuilder classBuilder) {
        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod<TMethodDefinition>(
            SyntheticIdentifier name,
            InvocationExpressionSyntax invocationToIntercept,
            Func<TMethodDefinition, Delegate> methodSelector,
            Action<TMethodDefinition>? inputBinder = null
        ) where TMethodDefinition : SyntheticInterceptorMethodDefinition, new() {
            var methodDef = new TMethodDefinition();
            inputBinder?.Invoke(methodDef);
            var selectedMethod = methodSelector(methodDef);
            methodDef.BindDynamicComponents();
            var operation = (IInvocationOperation)classBuilder.ΦCompilerApi.Context.Compilation
                .GetSemanticModel(invocationToIntercept.SyntaxTree).GetOperation(invocationToIntercept);
            return methodDef.InternalBindCompilerMethod(classBuilder, invocationToIntercept, operation, selectedMethod);
        }
    }
}