using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn.RendererExtensions;

namespace Mumei.CodeGen.Roslyn.Components;

internal sealed class RoslynSyntheticParameter(IParameterSymbol parameterSymbol) : ISyntheticParameter, ISyntheticConstructable<ParameterFragment> {
    public ISyntheticAttributeList? AttributesList { get; private init; } = parameterSymbol.GetAttributes().Length > 0
        ? ISyntheticAttributeList.From(parameterSymbol.GetAttributes())
        : null;

    public ISyntheticExpression? DefaultValue { get; private init; } = parameterSymbol.HasExplicitDefaultValue
        ? new RuntimeSyntheticLiteralExpression(parameterSymbol.ExplicitDefaultValue)
        : null;

    public ParameterFragment Construct(ICompilationUnitContext compilationUnit) {
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

        var attributesList = compilationUnit.Synthesize(AttributesList, AttributeListFragment.Empty);
        var defaultValue = compilationUnit.SynthesizeOptional<ExpressionFragment?>(DefaultValue);
        return new ParameterFragment {
            Name = parameterSymbol.Name,
            Type = parameterSymbol.Type.ToRenderFragment(),
            ParameterAttributes = paramAttributes,
            AttributesList = attributesList,
            DefaultValue = defaultValue
        };
    }
}