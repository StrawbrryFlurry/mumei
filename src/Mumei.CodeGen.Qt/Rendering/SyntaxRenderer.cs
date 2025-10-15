using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

internal sealed class SyntaxRenderTreeBuilder : GenericRenderTreeBuilder {
    private SyntaxWriter _sourceFile = new();

    protected override void TextCore(ReadOnlySpan<char> s) {
        _sourceFile.Write(s);
    }

    protected override void NewLineCore() {
        _sourceFile.WriteLine();
    }

    protected override void ValueCore<T>(in T value) {
        if (value is null) {
            _sourceFile.WriteLiteral("null");
            return;
        }

 #pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        _sourceFile.WriteLiteral(value);
 #pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    }

    protected override void BlockCore(string s) {
        _sourceFile.WriteBlock(s);
    }

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