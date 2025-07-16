using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtArgument : IQtTemplateBindable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}

public readonly struct QtArgumentList(
    QtArgument[] arguments
) : IQtTemplateBindable {
    public static QtArgumentList Builder(int length) {
        return new QtArgumentList(new QtArgument[length]);
    }

    internal QtArgument this[int idx] {
        set => arguments[idx] = value;
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}