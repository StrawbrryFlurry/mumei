namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct TriviaFragment(string? trivia) : IRenderFragment {
    public static readonly TriviaFragment Empty = new(null);

    public void Render(IRenderTreeBuilder renderTree) {
        if (trivia is null) {
            return;
        }

        renderTree.Text(trivia);
    }

    public static implicit operator TriviaFragment(string trivia) {
        return new TriviaFragment(trivia);
    }

    public override string ToString() {
        return DebugRenderer.Render(this);
    }
}