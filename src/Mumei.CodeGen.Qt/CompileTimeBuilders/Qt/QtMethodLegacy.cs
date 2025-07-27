using Mumei.CodeGen.Qt;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public static class QtMethodLegacy {
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

        public IQtCompileTimeValue Inject(IQtType type, int idx) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }
    }

    public sealed class MethodBuilderTypeArgumentsProvider {
        public Type Get(string name) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }
    }
}