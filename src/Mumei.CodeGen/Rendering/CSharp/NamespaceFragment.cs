using System.Collections.Immutable;

namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct NamespaceFragment(
    string? parentNamespace,
    string? name,
    ImmutableArray<ClassDeclarationFragment> classDeclarations,
    TriviaFragment leadingTrivia,
    TriviaFragment trailingTrivia
) : IRenderFragment {
    public static NamespaceFragment Empty => new(null, null, ImmutableArray<ClassDeclarationFragment>.Empty, TriviaFragment.Empty, TriviaFragment.Empty);

    public ImmutableArray<ClassDeclarationFragment> ClassDeclarations { get; } = classDeclarations;
    public string Name { get; } = name;
    public string? ParentNamespace { get; } = parentNamespace;

    public bool IsEmpty => name == null && ClassDeclarations.IsEmpty;

    public static NamespaceFragment Create(string name, ImmutableArray<ClassDeclarationFragment> classDeclarations) {
        return new NamespaceFragment(null, name, classDeclarations, TriviaFragment.Empty, TriviaFragment.Empty);
    }

    public NamespaceFragment WithLeadingTrivia(TriviaFragment trivia) {
        return new NamespaceFragment(
            ParentNamespace,
            Name,
            ClassDeclarations,
            trivia,
            trailingTrivia
        );
    }

    public NamespaceFragment WithTrailingTrivia(TriviaFragment trivia) {
        return new NamespaceFragment(
            ParentNamespace,
            Name,
            ClassDeclarations,
            leadingTrivia,
            trivia
        );
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Node(leadingTrivia);
        renderTree.Text("namespace");
        renderTree.Text(" ");

        if (ParentNamespace is not null) {
            renderTree.Text(ParentNamespace);
            renderTree.Text(".");
        }

        renderTree.Text(Name);
        renderTree.Text(" ");
        renderTree.StartCodeBlock();

        renderTree.List(ClassDeclarations.AsSpan());

        renderTree.EndCodeBlock();
        renderTree.Node(trailingTrivia);
    }
}