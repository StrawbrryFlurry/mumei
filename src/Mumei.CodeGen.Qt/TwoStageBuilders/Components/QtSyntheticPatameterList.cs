using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticParameterList(ReadOnlySpan<ISyntheticParameter> parameters) : ISyntheticParameterList, ISyntheticConstructable<ImmutableArray<ParameterFragment>> {
    private ISyntheticParameter[]? _parameters = parameters.Length > 0 ? [..parameters] : null;

    public static QtSyntheticParameterList FromMethodSymbol(IMethodSymbol method) {
        var parameters = new ISyntheticParameter[method.Parameters.Length];
        return new QtSyntheticParameterList(parameters);
    }

    public static QtSyntheticParameterList FromMethodInfo(MethodInfo method) {
        var methodParameters = method.GetParameters();
        var parameters = new ISyntheticParameter[methodParameters.Length];
        return new QtSyntheticParameterList(parameters);
    }

    public ImmutableArray<ParameterFragment> Construct(ISyntheticCompilation compilation) {
        if (_parameters is null or { Length: 0 }) {
            return ImmutableArray<ParameterFragment>.Empty;
        }

        var result = ImmutableArray.CreateBuilder<ParameterFragment>(_parameters.Length);
        foreach (var parameter in _parameters) {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (parameter is not ISyntheticConstructable<ParameterFragment> constructable) {
                throw new NotSupportedException();
            }
            result.Add(constructable.Construct(compilation));
        }

        return result.ToImmutable();
    }
}