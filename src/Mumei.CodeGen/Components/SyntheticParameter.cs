using System.Reflection;
using Mumei.CodeGen.Rendering.CSharp;
using ParameterAttributes = Mumei.CodeGen.Rendering.CSharp.ParameterAttributes;

namespace Mumei.CodeGen.Components;

internal sealed class SyntheticParameter(
    string name,
    ISyntheticType type,
    ISyntheticAttributeList? attributesList = null,
    ISyntheticExpression? defaultValue = null,
    ParameterAttributes attributes = ParameterAttributes.None
) : ISyntheticParameter, ISyntheticConstructable<ParameterFragment> {
    private string _name;
    public string Name { get; private init; } = name;

    public ISyntheticType Type { get; private init; } = type;
    public ParameterAttributes ParameterAttributes { get; }
    public ISyntheticAttributeList? AttributesList { get; private init; } = attributesList;
    public ISyntheticExpression? DefaultValue { get; private init; } = defaultValue;
    public ParameterAttributes Attributes { get; private init; } = attributes;

    public ParameterFragment Construct(ICompilationUnitContext compilationUnit) {
        var defaultValue = compilationUnit.SynthesizeOptional<ExpressionFragment?>(DefaultValue);
        var type = compilationUnit.Synthesize<TypeInfoFragment>(Type);

        return new ParameterFragment {
            Name = Name,
            DefaultValue = defaultValue,
            ParameterAttributes = Attributes,
            Type = type
        };
    }
}

internal sealed class RuntimeSyntheticParameter(ParameterInfo parameterInfo) : ISyntheticParameter, ISyntheticConstructable<ParameterFragment> {
    public ParameterFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
    public string Name { get; }
    public ISyntheticType Type { get; }
    public ParameterAttributes ParameterAttributes { get; }
}