using Mumei.CodeGen.Playground;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedParameter : IRenderNode {
    public required string Name { get; init; }
    public required ParameterAttributes Attributes { get; init; }
    public required SynthesizedTypeInfo Type { get; init; }

    public static SynthesizedParameter Create(
        SynthesizedTypeInfo type,
        string name,
        ParameterAttributes attributes = ParameterAttributes.None
    ) {
        return new SynthesizedParameter {
            Name = name,
            Type = type,
            Attributes = attributes
        };
    }

    public static SynthesizedParameter Create<T>(
        string name,
        ParameterAttributes attributes = ParameterAttributes.None
    ) {
        return new SynthesizedParameter {
            Name = name,
            Type = typeof(T),
            Attributes = attributes
        };
    }

    public static implicit operator SynthesizedParameter((SynthesizedTypeInfo Type, string Name) paramDescriptor) {
        return Create(paramDescriptor.Type, paramDescriptor.Name);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"{Attributes.List}{Type.FullName} {Name}");
    }
}