using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class QtSyntheticParameterList(ReadOnlySpan<ISyntheticParameter> parameters) : ISyntheticParameterList, ISyntheticConstructable<ImmutableArray<ParameterFragment>> {
    private ISyntheticParameter[]? _parameters = parameters.Length > 0 ? [..parameters] : null;

    public static QtSyntheticParameterList FromMethodSymbol(IMethodSymbol method) {
        var parameters = new ISyntheticParameter[method.Parameters.Length];

        for (var i = 0; i < method.Parameters.Length; i++) {
            var parameter = method.Parameters[i];
            parameters[i] = new RoslynSyntheticParameter(parameter);
        }

        return new QtSyntheticParameterList(parameters);
    }

    public static QtSyntheticParameterList FromMethodInfo(MethodInfo method) {
        var methodParameters = method.GetParameters();
        var parameters = new ISyntheticParameter[methodParameters.Length];

        for (var i = 0; i < methodParameters.Length; i++) {
            var parameter = methodParameters[i];
            parameters[i] = new RuntimeSyntheticParameter(parameter);
        }

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