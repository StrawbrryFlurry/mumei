using System.Diagnostics;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct CodeBlockNode : IRenderNode {
    private readonly string? _code;
    private readonly IRenderNode? _dynamicBlock;

    private CodeBlockNode(string? code, IRenderNode? dynamicBlock, bool allowEmpty = false) {
        _code = code;
        _dynamicBlock = dynamicBlock;

        Debug.Assert(_code is not null || _dynamicBlock is not null || allowEmpty);
    }

    public static CodeBlockNode ForCode(string code) {
        return new CodeBlockNode(code, null);
    }

    public static CodeBlockNode ForNode(IRenderNode dynamicBlock) {
        return new CodeBlockNode(null, dynamicBlock);
    }

    public static CodeBlockNode Empty() {
        return new CodeBlockNode(null, null, true);
    }

    public void Render(IRenderTreeBuilder context) {
        if (_code is not null) {
            context.Text(_code);
            return;
        }

        if (_dynamicBlock is not null) {
            context.Node(_dynamicBlock);
            return;
        }
    }
}

public readonly struct QtCodeBlock : IQtTemplateBindable {
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