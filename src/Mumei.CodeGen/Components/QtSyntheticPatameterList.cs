using System.Collections;
using System.Reflection;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

internal sealed class SyntheticParameterList(ReadOnlySpan<ISyntheticParameter> parameters) : ISyntheticParameterList, ISyntheticConstructable<ImmutableArray<ParameterFragment>> {
    private ISyntheticParameter[]? _parameters = parameters.Length > 0 ? [..parameters] : null;

    public static SyntheticParameterList FromMethodInfo(MethodInfo method) {
        var methodParameters = method.GetParameters();
        var parameters = new ISyntheticParameter[methodParameters.Length];

        for (var i = 0; i < methodParameters.Length; i++) {
            var parameter = methodParameters[i];
            parameters[i] = new RuntimeSyntheticParameter(parameter);
        }

        return new SyntheticParameterList(parameters);
    }

    public int Count => _parameters?.Length ?? 0;
    public ISyntheticParameter this[int index] => _parameters?[index] ?? Array.Empty<ISyntheticParameter>()[index];

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

    public ReadOnlySpan<ISyntheticParameter> AsSpan() {
        return _parameters is not null ? _parameters.AsSpan() : ReadOnlySpan<ISyntheticParameter>.Empty;
    }

    public IEnumerator<ISyntheticParameter> GetEnumerator() {
        return (_parameters ?? []).GetEnumeratorInterfaceImplementation();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}