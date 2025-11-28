namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct CompilationUnitFragment(
    TriviaFragment leadingTrivia,
    ImmutableArray<NamespaceFragment> namespaces,
    TriviaFragment trailingTrivia
) : IRenderFragment {
    public ImmutableArray<NamespaceFragment> Namespaces { get; } = namespaces;

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Node(leadingTrivia);
        renderTree.List(Namespaces.AsSpan());
        renderTree.Node(trailingTrivia);
    }

    public CompilationUnitFragment AddNamespace(NamespaceFragment namespaceFragment) {
        var newNamespaces = Namespaces.Add(namespaceFragment);
        return new CompilationUnitFragment(
            leadingTrivia, newNamespaces, trailingTrivia
        );
    }

    public CompilationUnitFragment WithLeadingTrivia(TriviaFragment trivia) {
        return new CompilationUnitFragment(
            trivia,
            Namespaces,
            trailingTrivia
        );
    }

    public CompilationUnitFragment WithTrailingTrivia(TriviaFragment trivia) {
        return new CompilationUnitFragment(
            leadingTrivia,
            Namespaces,
            trivia
        );
    }
}