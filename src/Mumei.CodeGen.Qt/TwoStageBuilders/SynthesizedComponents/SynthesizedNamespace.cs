using System.Collections.Immutable;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedNamespace(
    string? parentNamespace,
    string? name,
    ImmutableArray<SynthesizedClassDeclaration> classDeclarations
) : IRenderNode {
    public static SynthesizedNamespace Empty => new(null, null, ImmutableArray<SynthesizedClassDeclaration>.Empty);

    public ImmutableArray<SynthesizedClassDeclaration> ClassDeclarations { get; } = classDeclarations;
    public string Name { get; } = name;
    public string? ParentNamespace { get; } = parentNamespace;

    public bool IsEmpty => name == null && ClassDeclarations.IsEmpty;

    public static SynthesizedNamespace Create(string name, ImmutableArray<SynthesizedClassDeclaration> classDeclarations) {
        return new SynthesizedNamespace(null, name, classDeclarations);
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