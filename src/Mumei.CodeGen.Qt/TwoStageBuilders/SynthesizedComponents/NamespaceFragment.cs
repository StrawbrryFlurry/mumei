using System.Collections.Immutable;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct NamespaceFragment(
    string? parentNamespace,
    string? name,
    ImmutableArray<ClassDeclarationFragment> classDeclarations
) : IRenderFragment {
    public static NamespaceFragment Empty => new(null, null, ImmutableArray<ClassDeclarationFragment>.Empty);

    public ImmutableArray<ClassDeclarationFragment> ClassDeclarations { get; } = classDeclarations;
    public string Name { get; } = name;
    public string? ParentNamespace { get; } = parentNamespace;

    public bool IsEmpty => name == null && ClassDeclarations.IsEmpty;

    public static NamespaceFragment Create(string name, ImmutableArray<ClassDeclarationFragment> classDeclarations) {
        return new NamespaceFragment(null, name, classDeclarations);
    }

    public void Render(IRenderTreeBuilder renderTree) {
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
    }
}