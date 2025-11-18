using System.Collections.Immutable;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticTypeParameterList : ISyntheticConstructable<TypeParameterListFragment> {
    private List<ISyntheticTypeParameter>? _typeParameters;

    public TypeParameterListFragment Construct() {
        if (_typeParameters is null || _typeParameters.Count == 0) {
            return TypeParameterListFragment.Empty;
        }

        var result = ImmutableArray.CreateBuilder<TypeParameterFragment>(_typeParameters.Count);
        foreach (var typeParameter in _typeParameters) {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (typeParameter is not ISyntheticConstructable<TypeParameterFragment> constructable) {
                throw new NotSupportedException();
            }

            result.Add(constructable.Construct());
        }

        return new TypeParameterListFragment(result.ToImmutable());
    }
}