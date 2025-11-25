using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;
using ParameterAttributes = Mumei.CodeGen.Playground.ParameterAttributes;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticParameter(
    string name,
    ISyntheticType type,
    ISyntheticAttributeList? attributesList = null,
    ISyntheticExpression? defaultValue = null,
    ParameterAttributes attributes = ParameterAttributes.None
) : ISyntheticParameter, ISyntheticConstructable<ParameterFragment> {
    public string Name { get; private init; } = name;
    public ISyntheticType Type { get; private init; } = type;
    public ISyntheticAttributeList? AttributesList { get; private init; } = attributesList;
    public ISyntheticExpression? DefaultValue { get; private init; } = defaultValue;
    public ParameterAttributes Attributes { get; private init; } = attributes;

    public ParameterFragment Construct(ISyntheticCompilation compilation) {
        var defaultValue = compilation.SynthesizeOptional<ExpressionFragment?>(DefaultValue);
        var type = compilation.Synthesize<TypeInfoFragment>(Type);

        return new ParameterFragment {
            Name = Name,
            DefaultValue = defaultValue,
            Attributes = Attributes,
            Type = type
        };
    }
}

internal sealed class RuntimeSyntheticParameter(ParameterInfo parameterInfo) : ISyntheticParameter, ISyntheticConstructable<ParameterFragment> {
    public ParameterFragment Construct(ISyntheticCompilation compilation) {
        throw new NotImplementedException();
    }
}

internal sealed class RoslynSyntheticParameter(IParameterSymbol parameterSymbol) : ISyntheticParameter, ISyntheticConstructable<ParameterFragment> {
    public ParameterFragment Construct(ISyntheticCompilation compilation) {
        throw new NotImplementedException();
    }
}