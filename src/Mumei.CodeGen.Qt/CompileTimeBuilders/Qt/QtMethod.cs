using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public sealed class QtMethod<TReturnType> : IQtInvokable<TReturnType> {
    public QtMethod<TReturnType> WithName(string name) {
        return this;
    }

    public QtMethod<TReturnType> WithTypeParameters(params QtTypeParameter[] typeParameters) {
        return this;
    }

    public QtMethod<TReturnType> WithParameters(params IQtParameter[] parameters) {
        return this;
    }

    public TReturnType Invoke(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TDynamicReturnType DynamicInvoke<TDynamicReturnType>(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public static class QtMethod {
    public sealed class QtProxyMethodBuilderCtx { }

    public sealed class QtMethodBuilderCtx {
        public MethodBuilderArgumentsProvider Arguments { get; }
        public MethodBuilderTypeArgumentsProvider TypeArguments { get; set; }

        public T Interpolate<T>(IQtSyntaxTemplateInterpolator interpolator) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }

        public void ForEach<T>(T[] source, Action<T, QtMethodBuilderCtx> action) where T : IQtTemplateBindable {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }

        public T Return<T>(T value) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }
    }

    public sealed class MethodBuilderArgumentsProvider {
        public T Inject<T>(int idx) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }

        public IQtCompileTimeValue<Arg.T1> Inject(IQtType type, int idx) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }
    }

    public sealed class MethodBuilderTypeArgumentsProvider {
        public Type Get(string name) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }
    }
}