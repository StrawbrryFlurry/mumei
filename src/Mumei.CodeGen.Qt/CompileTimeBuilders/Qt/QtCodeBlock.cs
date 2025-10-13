using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtCodeBlock : IQtTemplateBindable, IRenderNode {
    private readonly string? _code;
    private readonly IQtTemplateBindable? _dynamicBlock;

    private QtCodeBlock(string? code, IQtTemplateBindable? dynamicBlock) {
        _code = code;
        _dynamicBlock = dynamicBlock;
    }

    public static QtCodeBlock ForCode(string code) {
        return new QtCodeBlock(code, null);
    }

    public static QtCodeBlock ForCode(IQtTemplateBindable dynamicBlock) {
        return new QtCodeBlock(null, dynamicBlock);
    }

    public static QtCodeBlock ForCode(FormattableSyntaxWritable code) {
        var outputWriter = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
        code.CopyToAndDispose(ref outputWriter);
        return new QtCodeBlock(outputWriter.ToString(), null);
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(_code);
    }

    public static implicit operator ReadOnlySpan<char>(QtCodeBlock block) {
        return block._code;
    }

    public void Render(IRenderTreeBuilder renderTree) {
        if (_code is not null) {
            renderTree.Text(_code);
        } else if (_dynamicBlock is not null) {
            renderTree.Bind(_dynamicBlock);
        }
    }
}