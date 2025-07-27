using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtArgument : IQtTemplateBindable {
    public required QtExpression Expression { get; init; }
    public string? NamedIdentifier { get; init; }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (NamedIdentifier is not null) {
            writer.Write(NamedIdentifier);
            writer.Write(": ");
        }

        writer.Write(Expression);
    }
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