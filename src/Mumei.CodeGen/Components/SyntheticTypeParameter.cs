using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class QtSyntheticTypeParameter(string name) : ISyntheticTypeParameter, ISyntheticConstructable<TypeParameterFragment> {
    public string Name { get; init; } = name;

    public sealed class Constraint(ISyntheticType typeConstraint) : ISyntheticTypeParameter.IConstraint {
        public ISyntheticType TypeConstraint { get; init; } = typeConstraint;
    }

    public TypeParameterFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
}

internal sealed class RuntimeSyntheticTypeParameter(Type typeParameterType) : ISyntheticTypeParameter, ISyntheticConstructable<TypeParameterFragment> {
    public string Name { get; init; } = typeParameterType.Name;

    // Figure out constraints from base type and interfaces
    public sealed class Constraint(Type typeConstraint) : ISyntheticTypeParameter.IConstraint { }

    public TypeParameterFragment Construct(ICompilationUnitContext compilationUnit) {
        return new TypeParameterFragment();
    }
}