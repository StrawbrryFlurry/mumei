using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Playground;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct ParameterFragment : IRenderFragment {
    public required ExpressionFragment Name { get; init; }
    public AttributeListFragment AttributesList { get; init; }
    public ExpressionFragment? DefaultValue { get; init; }
    public ParameterAttributes ParameterAttributes { get; init; }
    public required TypeInfoFragment Type { get; init; }

    public static ParameterFragment Create(
        TypeInfoFragment type,
        ExpressionFragment name,
        ParameterAttributes parameterAttributes = ParameterAttributes.None
    ) {
        return new ParameterFragment {
            Name = name,
            Type = type,
            ParameterAttributes = parameterAttributes
        };
    }

    public static ParameterFragment Create(
        TypeInfoFragment type,
        ExpressionFragment name,
        ParameterAttributes parameterAttributes,
        out ExpressionFragment local
    ) {
        local = name;
        return new ParameterFragment {
            Name = name,
            Type = type,
            ParameterAttributes = parameterAttributes
        };
    }

    public static ParameterFragment Create(
        TypeInfoFragment type,
        ExpressionFragment name,
        out ExpressionFragment local
    ) {
        local = name;
        return new ParameterFragment {
            Name = name,
            Type = type,
            ParameterAttributes = ParameterAttributes.None
        };
    }

    public static ParameterFragment Create(
        ITypeSymbol type,
        ExpressionFragment name,
        ParameterAttributes parameterAttributes = ParameterAttributes.None
    ) {
        return new ParameterFragment {
            Name = name,
            Type = new TypeInfoFragment(type),
            ParameterAttributes = parameterAttributes
        };
    }

    public static ParameterFragment Create<T>(
        ExpressionFragment name,
        ParameterAttributes attributes = ParameterAttributes.None
    ) {
        return new ParameterFragment {
            Name = name,
            Type = typeof(T),
            ParameterAttributes = attributes
        };
    }

    public static implicit operator ParameterFragment((TypeInfoFragment Type, ExpressionFragment Name) paramDescriptor) {
        return Create(paramDescriptor.Type, paramDescriptor.Name);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"{ParameterAttributes.List}{Type.FullName} {Name}");
        if (DefaultValue is { } defaultValueExpression) {
            renderTree.Interpolate($" = {defaultValueExpression}");
        }
    }
}