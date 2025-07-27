using Mumei.CodeGen.Qt;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public abstract class QtClassTemplate<TSelf> : IQtThis where TSelf : QtClassTemplate<TSelf> {
    public IEnumerable<TE> CompTimeIterate<TE>(IEnumerable<TE> compileTimeIterableSelector) where TE : IQtTemplateBindable {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T As<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        throw new NotImplementedException();
    }

    public T Is<T>() {
        throw new NotImplementedException();
    }
}