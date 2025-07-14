using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public sealed class QtProperty<T> : IQtCompileTimeValue<T>, IQtTemplateBindable {
    public TActual As<TActual>() {
        throw new NotImplementedException();
    }
}

public static class QtProperty {
    public sealed class QtPropertyBuilderCtx<T> {
        public IQtThis This { get; set; }
        public T Value { get; set; }
    }
}