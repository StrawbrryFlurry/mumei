using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering.CSharp;
using ParameterAttributes = Mumei.CodeGen.Rendering.CSharp.ParameterAttributes;

namespace Mumei.CodeGen.Components;

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
            ParameterAttributes = Attributes,
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
    public ISyntheticAttributeList? AttributesList { get; private init; } = parameterSymbol.GetAttributes().Length > 0
        ? QtSyntheticAttributeList.FromAttributeData(parameterSymbol.GetAttributes())
        : null;

    public ISyntheticExpression? DefaultValue { get; private init; } = parameterSymbol.HasExplicitDefaultValue
        ? new RuntimeSyntheticLiteralExpression(parameterSymbol.ExplicitDefaultValue)
        : null;

    public ParameterFragment Construct(ISyntheticCompilation compilation) {
        var paramAttributes = ParameterAttributes.None;
        if (parameterSymbol.RefKind == RefKind.Ref) {
            paramAttributes |= ParameterAttributes.Ref;
        } else if (parameterSymbol.RefKind == RefKind.RefReadOnlyParameter) {
            paramAttributes |= ParameterAttributes.Ref | ParameterAttributes.Readonly;
        } else if (parameterSymbol.RefKind == RefKind.Out) {
            paramAttributes |= ParameterAttributes.Out;
        } else if (parameterSymbol.IsParams) {
            paramAttributes |= ParameterAttributes.Params;
        } else if (parameterSymbol.RefKind == RefKind.In) {
            paramAttributes |= ParameterAttributes.In;
        }

        if (parameterSymbol.IsThis) {
            paramAttributes |= ParameterAttributes.This;
        }

        var attributesList = compilation.Synthesize(AttributesList, AttributeListFragment.Empty);
        var defaultValue = compilation.SynthesizeOptional<ExpressionFragment?>(DefaultValue);
        return new ParameterFragment {
            Name = parameterSymbol.Name,
            Type = new TypeInfoFragment(parameterSymbol.Type),
            ParameterAttributes = paramAttributes,
            AttributesList = attributesList,
            DefaultValue = defaultValue
        };
    }
}