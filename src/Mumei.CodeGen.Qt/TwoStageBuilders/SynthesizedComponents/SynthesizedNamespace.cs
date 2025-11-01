using System.Collections.Immutable;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedNamespace(
    string? parentNamespace,
    string name,
    ImmutableArray<SynthesizedClass> classDeclarations
) : IRenderNode {
    public ImmutableArray<SynthesizedClass> ClassDeclarations { get; } = classDeclarations;
    public string Name { get; } = name;
    public string? ParentNamespace { get; } = parentNamespace;

    public static SynthesizedNamespace Create(string name, ImmutableArray<SynthesizedClass> classDeclarations) {
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