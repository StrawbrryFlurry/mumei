namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct CompilationUnitFragment(
    TriviaFragment leadingTrivia,
    ImmutableArray<NamespaceOrGlobalScopeFragment> namespaces,
    TriviaFragment trailingTrivia
) : IRenderFragment {
    public ImmutableArray<NamespaceOrGlobalScopeFragment> Namespaces { get; } = namespaces;

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Node(leadingTrivia);
        renderTree.MemberList(Namespaces.AsSpan());
        renderTree.Node(trailingTrivia);
    }

    public CompilationUnitFragment AddNamespace(NamespaceOrGlobalScopeFragment namespaceFragment) {
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