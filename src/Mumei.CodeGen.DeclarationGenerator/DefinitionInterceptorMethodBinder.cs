using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Roslyn.Components;

namespace Mumei.CodeGen.DeclarationGenerator;

public static class InternalDefinitionInterceptorMethodBinder {
    public static ISyntheticMethodBuilder<Delegate> BindInterceptorMethod<TMethodDefinition>(
        this ISimpleSyntheticClassBuilder builder,
        TMethodDefinition methodDefinition,
        InvocationExpressionSyntax invocationToBind,
        IMethodSymbol method
    ) where TMethodDefinition : SyntheticInterceptorMethodDefinition {
        builder.ΦCompilerApi.Context.InterceptLocationAttribute();
        return methodDefinition.InternalBindCompilerMethod(
            builder,
            targetMethod
        );
    }
}