namespace Mumei.CodeGen.Components;

internal sealed class SyntheticMethodBuilder<TSignature>(
    string name,
    IΦInternalClassBuilderCompilerApi classApi
) : SyntheticMethodBase<ISyntheticMethodBuilder<TSignature>>(name, classApi), ISyntheticMethodBuilder<TSignature> where TSignature : Delegate {
    // TODO: Use MethodRef here?
    public TSignature Bind(object target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TDynamicSignature BindAs<TDynamicSignature>(object target) where TDynamicSignature : Delegate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticMethodBuilder<TSignature> WithBody(TSignature bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticMethodBuilder<TSignature> WithBody<TDeps>(TDeps deps, Func<TDeps, TSignature> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticMethodBuilder<TSignature> WithBody(ISyntheticCodeBlock body) {
        _body = body;
        return this;
    }
}