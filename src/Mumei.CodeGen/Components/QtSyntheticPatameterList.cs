using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class QtSyntheticParameterList(ReadOnlySpan<ISyntheticParameter> parameters) : ISyntheticParameterList, ISyntheticConstructable<ImmutableArray<ParameterFragment>> {
    private ISyntheticParameter[]? _parameters = parameters.Length > 0 ? [..parameters] : null;

    public static QtSyntheticParameterList FromMethodInfo(MethodInfo method) {
        var methodParameters = method.GetParameters();
        var parameters = new ISyntheticParameter[methodParameters.Length];

        for (var i = 0; i < methodParameters.Length; i++) {
            var parameter = methodParameters[i];
            parameters[i] = new RuntimeSyntheticParameter(parameter);
        }

        return new QtSyntheticParameterList(parameters);
    }

    public ImmutableArray<ParameterFragment> Construct(ICompilationUnitContext compilationUnit) {
        if (_parameters is null or { Length: 0 }) {
            return ImmutableArray<ParameterFragment>.Empty;
        }

        var result = ImmutableArray.CreateBuilder<ParameterFragment>(_parameters.Length);
        foreach (var parameter in _parameters) {
            result.Add(compilationUnit.Synthesize<ParameterFragment>(parameter));
        }

        return result.ToImmutable();
    }

    public void InsertAt(int i, ISyntheticParameter parameter) {
        if (_parameters is null) {
            _parameters = [parameter];
            return;
        }

        var newParameters = new ISyntheticParameter[_parameters.Length + 1];
        Array.Copy(_parameters, 0, newParameters, 0, i);
        newParameters[i] = parameter;
        Array.Copy(_parameters, i, newParameters, i + 1, _parameters.Length);

        _parameters = newParameters;
    }
}