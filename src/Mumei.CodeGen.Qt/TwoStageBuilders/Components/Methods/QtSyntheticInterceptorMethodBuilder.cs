using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticInterceptorMethodBuilder<TSignature>(
    IλInternalClassBuilderCompilerApi classApi
) : QtSyntheticMethodBase<ISyntheticInterceptorMethodBuilder<TSignature>>(classApi), ISyntheticInterceptorMethodBuilder<TSignature> where TSignature : Delegate {
    public static QtSyntheticInterceptorMethodBuilder<TSignature> CreateFromInterceptionTargetInvocation(
        InvocationExpressionSyntax invocationToIntercept,
        IλInternalClassBuilderCompilerApi classApi
    ) {
        var builder = new QtSyntheticInterceptorMethodBuilder<TSignature>(classApi);
        return builder;
    }

    public TSignature Bind(object target) {
        throw new NotImplementedException();
    }

    public TSignature1 BindAs<TSignature1>(object target) where TSignature1 : Delegate {
        throw new NotImplementedException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(TSignature bodyImpl) {
        throw new NotImplementedException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(Func<IInterceptedMethodContext, TSignature> bodyImpl) {
        throw new NotImplementedException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(Action<IInterceptedMethodContext> bodyImpl) {
        throw new NotImplementedException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Func<TInputs, IInterceptedMethodContext, TSignature> bodyImpl) {
        throw new NotImplementedException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Action<TInputs, IInterceptedMethodContext> bodyImpl) {
        throw new NotImplementedException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(ISyntheticCodeBlock body) {
        throw new NotImplementedException();
    }
}