using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct NamespaceOrGlobalScopeFragment(
    string? parentNamespace,
    string? name,
    ImmutableArray<ClassDeclarationFragment> classDeclarations,
    ImmutableArray<NamespaceOrGlobalScopeFragment> namespaceDeclarations,
    TriviaFragment leadingTrivia,
    TriviaFragment trailingTrivia
) : IRenderFragment {
    public static NamespaceOrGlobalScopeFragment GlobalScope => new(
        null,
        null,
        ImmutableArray<ClassDeclarationFragment>.Empty,
        ImmutableArray<NamespaceOrGlobalScopeFragment>.Empty,
        TriviaFragment.Empty,
        TriviaFragment.Empty
    );

    public ImmutableArray<ClassDeclarationFragment> ClassDeclarations { get; } = classDeclarations;
    public ImmutableArray<NamespaceOrGlobalScopeFragment> NamespaceDeclarations { get; } = namespaceDeclarations;
    public string? Name { get; } = name;
    public string? ParentNamespace { get; } = parentNamespace;

    public bool IsGlobalScope => name is null;

    public NamespaceOrGlobalScopeFragment WithClassDeclarations(ImmutableArray<ClassDeclarationFragment> classDeclarations) {
        return new NamespaceOrGlobalScopeFragment(
            ParentNamespace,
            Name,
            classDeclarations,
            NamespaceDeclarations,
            leadingTrivia,
            trailingTrivia
        );
    }

    public static NamespaceOrGlobalScopeFragment Create(string name, ImmutableArray<ClassDeclarationFragment> classDeclarations) {
        return new NamespaceOrGlobalScopeFragment(
            null,
            name,
            classDeclarations,
            ImmutableArray<NamespaceOrGlobalScopeFragment>.Empty,
            TriviaFragment.Empty,
            TriviaFragment.Empty
        );
    }

    public NamespaceOrGlobalScopeFragment WithLeadingTrivia(TriviaFragment trivia) {
        return new NamespaceOrGlobalScopeFragment(
            ParentNamespace,
            Name,
            ClassDeclarations,
            NamespaceDeclarations,
            trivia,
            trailingTrivia
        );
    }

    public NamespaceOrGlobalScopeFragment WithTrailingTrivia(TriviaFragment trivia) {
        return new NamespaceOrGlobalScopeFragment(
            ParentNamespace,
            Name,
            ClassDeclarations,
            NamespaceDeclarations,
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
        var hadPreviousMember = false;
        renderTree.MemberList(ClassDeclarations.AsSpan(), ref hadPreviousMember);
        renderTree.MemberList(NamespaceDeclarations.AsSpan(), ref hadPreviousMember);
    }
}