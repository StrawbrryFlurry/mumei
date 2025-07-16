using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtTypeArgument : IQtTemplateBindable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}

public readonly struct QtTypeArgumentList : IQtTemplateBindable {
    public bool IsEmpty => true;

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}