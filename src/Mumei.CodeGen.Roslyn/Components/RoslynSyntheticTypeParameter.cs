using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn.RendererExtensions;

namespace Mumei.CodeGen.Roslyn.Components;

internal sealed class RoslynSyntheticTypeParameter(ITypeParameterSymbol parameter) : ISyntheticTypeParameter, ISyntheticConstructable<TypeParameterFragment> {
    public string Name { get; init; } = parameter.Name;

    public sealed class Constraint(ISyntheticType typeConstraint) : ISyntheticTypeParameter.IConstraint {
        public ISyntheticType TypeConstraint { get; init; } = typeConstraint;
    }

    public TypeParameterFragment Construct(ICompilationUnitContext compilationUnit) {
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
            constraints.Add(constraintType.ToRenderFragment()); // Nullable annotations are also included in the constraint type
        }

        return new TypeParameterFragment(
            Name,
            constraints.ToImmutable()
        );
    }
}