using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class QtSyntheticTypeParameterList(ReadOnlySpan<ISyntheticTypeParameter> typeParameters) : ISyntheticTypeParameterList, ISyntheticConstructable<TypeParameterListFragment> {
    private ISyntheticTypeParameter[]? _typeParameters = typeParameters.Length > 0 ? [..typeParameters] : null;

    public static QtSyntheticTypeParameterList FromMethodInfo(MethodInfo method) {
        var methodTypeParameters = method.GetGenericArguments();
        var typeParameters = new ISyntheticTypeParameter[methodTypeParameters.Length];

        for (var i = 0; i < methodTypeParameters.Length; i++) {
            var typeParameter = methodTypeParameters[i];
            typeParameters[i] = new RuntimeSyntheticTypeParameter(typeParameter);
        }

        return new QtSyntheticTypeParameterList(typeParameters);
    }

    public TypeParameterListFragment Construct(ICompilationUnitContext compilationUnit) {
        if (_typeParameters is null || _typeParameters.Length == 0) {
            return TypeParameterListFragment.Empty;
        }

        var result = ImmutableArray.CreateBuilder<TypeParameterFragment>(_typeParameters.Length);
        foreach (var typeParameter in _typeParameters) {
            result.Add(compilationUnit.Synthesize<TypeParameterFragment>(typeParameter));
        }

        return new TypeParameterListFragment(result.ToImmutable());
    }
}