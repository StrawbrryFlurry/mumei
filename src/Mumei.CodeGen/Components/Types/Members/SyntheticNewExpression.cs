using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen.Components;

public sealed class SyntheticNewExpression<TResult>(ISyntheticClass cls, object[] args) : IRenderFragment {
    public static implicit operator TResult(SyntheticNewExpression<TResult> value) {
        throw new NotSupportedException();
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text($"new {cls.Name}(..args)");
    }
}