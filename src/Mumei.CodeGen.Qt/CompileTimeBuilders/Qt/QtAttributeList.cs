using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtAttribute : IQtTemplateBindable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}

public readonly struct QtAttributeList : IQtTemplateBindable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}