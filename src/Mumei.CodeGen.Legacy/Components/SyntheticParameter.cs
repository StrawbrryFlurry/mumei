using System.Reflection;
using Mumei.CodeGen.Legacy.Rendering.CSharp;
using CSharp_ParameterAttributes = Mumei.CodeGen.Legacy.Rendering.CSharp.ParameterAttributes;
using ParameterAttributes = Mumei.CodeGen.Legacy.Rendering.CSharp.ParameterAttributes;

namespace Mumei.CodeGen.Legacy.Components;

internal sealed class SyntheticParameter(
    string name,
    ISyntheticType type,
    ISyntheticAttributeList? attributesList = null,
    ISyntheticExpression? defaultValue = null,
    CSharp_ParameterAttributes attributes = CSharp_ParameterAttributes.None
) : ISyntheticParameter, ISyntheticConstructable<ParameterFragment> {
    private string _name;
    public string Name { get; private init; } = name;

    public ISyntheticType Type { get; private init; } = type;
    public CSharp_ParameterAttributes ParameterAttributes { get; }
    public ISyntheticAttributeList? AttributesList { get; private init; } = attributesList;
    public ISyntheticExpression? DefaultValue { get; private init; } = defaultValue;
    public CSharp_ParameterAttributes Attributes { get; private init; } = attributes;

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

internal sealed class RuntimeSyntheticParameter(ParameterInfo parameterInfo)
    : ISyntheticParameter, ISyntheticConstructable<ParameterFragment> {
    public ParameterFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }

    public string Name { get; }
    public ISyntheticType Type { get; }
    public CSharp_ParameterAttributes ParameterAttributes { get; }
}