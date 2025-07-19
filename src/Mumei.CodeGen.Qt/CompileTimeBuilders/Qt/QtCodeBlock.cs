using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtCodeBlock(
    string code
) : IQtTemplateBindable {
    private readonly string _code = code;

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(_code);
    }

    public static implicit operator ReadOnlySpan<char>(QtCodeBlock block) {
        return block._code;
    }
}