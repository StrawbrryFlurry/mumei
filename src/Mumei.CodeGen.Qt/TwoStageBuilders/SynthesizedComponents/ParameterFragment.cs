using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Playground;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct ParameterFragment : IRenderFragment {
    public required string Name { get; init; }
    public ParameterAttributes Attributes { get; init; }
    public required TypeInfoFragment Type { get; init; }

    public static ParameterFragment Create(
        TypeInfoFragment type,
        string name,
        ParameterAttributes attributes = ParameterAttributes.None
    ) {
        return new ParameterFragment {
            Name = name,
            Type = type,
            Attributes = attributes
        };
    }

    public static ParameterFragment Create(
        ITypeSymbol type,
        string name,
        ParameterAttributes attributes = ParameterAttributes.None
    ) {
        return new ParameterFragment {
            Name = name,
            Type = new TypeInfoFragment(type),
            Attributes = attributes
        };
    }

    public static ParameterFragment Create<T>(
        string name,
        ParameterAttributes attributes = ParameterAttributes.None
    ) {
        return new ParameterFragment {
            Name = name,
            Type = typeof(T),
            Attributes = attributes
        };
    }

    public static implicit operator ParameterFragment((TypeInfoFragment Type, string Name) paramDescriptor) {
        return Create(paramDescriptor.Type, paramDescriptor.Name);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"{Attributes.List}{Type.FullName} {Name}");
    }
}