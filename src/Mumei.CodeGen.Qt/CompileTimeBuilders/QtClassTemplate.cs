using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public abstract class QtClassTemplate<TSelf> : IQtThis where TSelf : QtClassTemplate<TSelf> {
    public IEnumerable<TE> CompTimeIterate<TE>(IEnumerable<TE> compileTimeIterableSelector) where TE : IQtTemplateBindable {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T As<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}