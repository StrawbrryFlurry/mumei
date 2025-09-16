using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

internal sealed class SyntaxRenderer : GenericRenderer {
    private SyntaxWriter _sourceFile = new();

    protected override void TextCore(ReadOnlySpan<char> s) {
        _sourceFile.Write(s);
    }

    protected override void BlockCore() { }

    protected override void StartBlockCore() {
        _sourceFile.Indent();
    }

    protected override void EndBlockCore() {
        _sourceFile.Dedent();
    }

    protected override void BindCore<TBindable>(TBindable bindable) {
        bindable.WriteSyntax(ref _sourceFile);
    }

    protected override void SyntaxNodeCore<TNode>(TNode node) { }

    protected override void NodeCore<TNode>(TNode renderable) {
        renderable.Render(this);
    }

    public string GetSourceText() {
        return _sourceFile.ToString();
    }
}