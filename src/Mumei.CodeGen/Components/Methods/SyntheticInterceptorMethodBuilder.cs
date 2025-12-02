namespace Mumei.CodeGen.Components;

internal sealed class SyntheticInterceptorMethodBuilder<TSignature>(
    SyntheticIdentifier name,
    IΦInternalClassBuilderCompilerApi classApi
) : SyntheticMethodBase<ISyntheticInterceptorMethodBuilder<TSignature>>(name, classApi), ISyntheticInterceptorMethodBuilder<TSignature> where TSignature : Delegate {
    public TSignature Bind(object target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TCast BindAs<TCast>(object target) where TCast : Delegate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(TSignature bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(Func<IInterceptedMethodContext, TSignature> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(Action<IInterceptedMethodContext> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Func<TInputs, IInterceptedMethodContext, TSignature> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Action<TInputs, IInterceptedMethodContext> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}