using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct NamespaceOrGlobalScopeFragment(
    string? parentNamespace,
    string? name,
    ImmutableArray<ClassDeclarationFragment> classDeclarations,
    TriviaFragment leadingTrivia,
    TriviaFragment trailingTrivia
) : IRenderFragment {
    public static NamespaceOrGlobalScopeFragment GlobalScope => new(null, null, ImmutableArray<ClassDeclarationFragment>.Empty, TriviaFragment.Empty, TriviaFragment.Empty);

    public ImmutableArray<ClassDeclarationFragment> ClassDeclarations { get; } = classDeclarations;
    public string? Name { get; } = name;
    public string? ParentNamespace { get; } = parentNamespace;

    public bool IsGlobalScope => name is null;

    public NamespaceOrGlobalScopeFragment WithClassDeclarations(ImmutableArray<ClassDeclarationFragment> classDeclarations) {
        return new NamespaceOrGlobalScopeFragment(
            ParentNamespace,
            Name,
            classDeclarations,
            leadingTrivia,
            trailingTrivia
        );
    }

    public static NamespaceOrGlobalScopeFragment Create(string name, ImmutableArray<ClassDeclarationFragment> classDeclarations) {
        return new NamespaceOrGlobalScopeFragment(null, name, classDeclarations, TriviaFragment.Empty, TriviaFragment.Empty);
    }

    public NamespaceOrGlobalScopeFragment WithLeadingTrivia(TriviaFragment trivia) {
        return new NamespaceOrGlobalScopeFragment(
            ParentNamespace,
            Name,
            ClassDeclarations,
            trivia,
            trailingTrivia
        );
    }

    public NamespaceOrGlobalScopeFragment WithTrailingTrivia(TriviaFragment trivia) {
        return new NamespaceOrGlobalScopeFragment(
            ParentNamespace,
            Name,
            ClassDeclarations,
            leadingTrivia,
            trivia
        );
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Node(leadingTrivia);

        if (IsGlobalScope) {
            // Global scope
            RenderMembers(renderTree);

            renderTree.Node(trailingTrivia);
            return;
        }

        renderTree.Text("namespace");
        renderTree.Text(" ");

        if (ParentNamespace is not null) {
            renderTree.Text(ParentNamespace);
            renderTree.Text(".");
        }

        renderTree.Text(Name);
        renderTree.Text(" ");
        renderTree.StartCodeBlock();

        RenderMembers(renderTree);

        renderTree.EndCodeBlock();
        renderTree.Node(trailingTrivia);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderMembers(IRenderTreeBuilder renderTree) {
        renderTree.List(ClassDeclarations.AsSpan());
    }
}