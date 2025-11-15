using System.Diagnostics;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct CodeBlockFragment : IRenderFragment {
    private readonly string? _code;
    private readonly IRenderFragment? _dynamicBlock;

    private CodeBlockFragment(string? code, IRenderFragment? dynamicBlock, bool allowEmpty = false) {
        _code = code;
        _dynamicBlock = dynamicBlock;

        Debug.Assert(_code is not null || _dynamicBlock is not null || allowEmpty);
    }

    public static CodeBlockFragment ForCode(string code) {
        return new CodeBlockFragment(code, null);
    }

    public static CodeBlockFragment ForNode(IRenderFragment dynamicBlock) {
        return new CodeBlockFragment(null, dynamicBlock);
    }

    public static CodeBlockFragment Empty() {
        return new CodeBlockFragment(null, null, true);
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