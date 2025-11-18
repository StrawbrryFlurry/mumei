using System.Collections.Immutable;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticParameterList(ISyntheticParameter[] parameters) : ISyntheticConstructable<ImmutableArray<ParameterFragment>> {
    private List<ISyntheticParameter>? _parameters = parameters.Length > 0 ? new List<ISyntheticParameter>(parameters) : null;

    public ImmutableArray<ParameterFragment> Construct() {
        if (_parameters is null or { Count: 0 }) {
            return ImmutableArray<ParameterFragment>.Empty;
        }

        var result = ImmutableArray.CreateBuilder<ParameterFragment>(_parameters.Count);
        foreach (var parameter in _parameters) {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (parameter is not ISyntheticConstructable<ParameterFragment> constructable) {
                throw new NotSupportedException();
            }
            result.Add(constructable.Construct());
        }

        return result.ToImmutable();
    }
}