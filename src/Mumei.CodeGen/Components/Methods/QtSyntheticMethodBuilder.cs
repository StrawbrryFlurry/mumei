using Mumei.CodeGen.Components.Types;

namespace Mumei.CodeGen.Components.Methods;

internal sealed class QtSyntheticMethodBuilder<TSignature>(string name, IλInternalClassBuilderCompilerApi classApi) : QtSyntheticMethodBase<ISyntheticMethodBuilder<TSignature>>(name, classApi), ISyntheticMethodBuilder<TSignature> where TSignature : Delegate {
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