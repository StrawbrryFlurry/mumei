using Mumei.CodeGen.Playground;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedParameter : IRenderNode {
    public required string Name { get; init; }
    public required ParameterAttributes Attributes { get; init; }
    public required SynthesizedTypeInfo Type { get; init; }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"{Attributes.List}{Type.FullName} {Name}");
    }
}