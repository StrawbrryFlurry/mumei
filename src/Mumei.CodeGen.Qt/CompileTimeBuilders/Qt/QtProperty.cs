using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public sealed class QtProperty<T> : IQtCompileTimeValue, IQtTemplateBindable {
    public TActual As<TActual>() {
        throw new NotImplementedException();
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}

public static class QtProperty {
    public sealed class QtPropertyBuilderCtx<T> {
        public required IQtThis This { get; set; }
        public required T Value { get; set; }
    }
}