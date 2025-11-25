using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticTypeParameter(string name) : ISyntheticTypeParameter, ISyntheticConstructable<TypeParameterFragment> {
    public string Name { get; init; } = name;

    public sealed class Constraint(ISyntheticType typeConstraint) : ISyntheticTypeParameter.IConstraint {
        public ISyntheticType TypeConstraint { get; init; } = typeConstraint;
    }

    public TypeParameterFragment Construct(ISyntheticCompilation compilation) {
        throw new NotImplementedException();
    }
}

internal sealed class RuntimeSyntheticTypeParameter(Type typeParameterType) : ISyntheticTypeParameter, ISyntheticConstructable<TypeParameterFragment> {
    public string Name { get; init; } = typeParameterType.Name;

    // Figure out constraints from base type and interfaces
    public sealed class Constraint(Type typeConstraint) : ISyntheticTypeParameter.IConstraint { }

    public TypeParameterFragment Construct(ISyntheticCompilation compilation) {
        return new TypeParameterFragment();
    }
}

internal sealed class RoslynSyntheticTypeParameter(ITypeParameterSymbol parameter) : ISyntheticTypeParameter, ISyntheticConstructable<TypeParameterFragment> {
    public string Name { get; init; } = parameter.Name;

    public sealed class Constraint(ISyntheticType typeConstraint) : ISyntheticTypeParameter.IConstraint {
        public ISyntheticType TypeConstraint { get; init; } = typeConstraint;
    }

    public TypeParameterFragment Construct(ISyntheticCompilation compilation) {
        var constraints = ImmutableArray.CreateBuilder<TypeParameterFragment.Constraint>(parameter.ConstraintTypes.Length);

        if (parameter.HasReferenceTypeConstraint) {
            constraints.Add(
                parameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
                    ? TypeParameterFragment.Constraint.NullableClass
                    : TypeParameterFragment.Constraint.Class
            );
        }

        if (parameter.HasValueTypeConstraint) {
            constraints.Add(TypeParameterFragment.Constraint.Struct);
        }

        if (parameter.HasConstructorConstraint) {
            constraints.Add(TypeParameterFragment.Constraint.New);
        }

        if (parameter.HasUnmanagedTypeConstraint) {
            constraints.Add(TypeParameterFragment.Constraint.Unmanaged);
        }

        if (parameter.HasNotNullConstraint) {
            constraints.Add(TypeParameterFragment.Constraint.NotNull);
        }

        foreach (var constraintType in parameter.ConstraintTypes) {
            constraints.Add(new TypeInfoFragment(constraintType)); // Nullable annotations are also included in the constraint type
        }

        return new TypeParameterFragment(
            Name,
            constraints.ToImmutable()
        );
    }
}