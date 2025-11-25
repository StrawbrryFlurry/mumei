using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticTypeParameterList(ReadOnlySpan<ISyntheticTypeParameter> typeParameters) : ISyntheticTypeParameterList, ISyntheticConstructable<TypeParameterListFragment> {
    private ISyntheticTypeParameter[]? _typeParameters = typeParameters.Length > 0 ? [..typeParameters] : null;

    public static QtSyntheticTypeParameterList FromMethodSymbol(IMethodSymbol method) {
        var typeParameters = new ISyntheticTypeParameter[method.TypeParameters.Length];
        for (var i = 0; i < method.TypeParameters.Length; i++) {
            var typeParameter = method.TypeParameters[i];
            typeParameters[i] = new RoslynSyntheticTypeParameter(typeParameter);
        }

        return new QtSyntheticTypeParameterList(typeParameters);
    }

    public static QtSyntheticTypeParameterList FromMethodInfo(MethodInfo method) {
        var methodTypeParameters = method.GetGenericArguments();
        var typeParameters = new ISyntheticTypeParameter[methodTypeParameters.Length];

        for (var i = 0; i < methodTypeParameters.Length; i++) {
            var typeParameter = methodTypeParameters[i];
            typeParameters[i] = new RuntimeSyntheticTypeParameter(typeParameter);
        }

        return new QtSyntheticTypeParameterList(typeParameters);
    }

    public TypeParameterListFragment Construct(ISyntheticCompilation compilation) {
        if (_typeParameters is null || _typeParameters.Length == 0) {
            return TypeParameterListFragment.Empty;
        }

        var result = ImmutableArray.CreateBuilder<TypeParameterFragment>(_typeParameters.Length);
        foreach (var typeParameter in _typeParameters) {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (typeParameter is not ISyntheticConstructable<TypeParameterFragment> constructable) {
                throw new NotSupportedException();
            }

            result.Add(constructable.Construct(compilation));
        }

        return new TypeParameterListFragment(result.ToImmutable());
    }
}