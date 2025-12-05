using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class QtSyntheticTypeParameter(SyntheticIdentifier name) : ISyntheticTypeParameter, ISyntheticConstructable<TypeParameterFragment> {
    public SyntheticIdentifier Name { get; init; } = name;

    public sealed class Constraint(ISyntheticType typeConstraint) : ISyntheticTypeParameter.IConstraint {
        public ISyntheticType TypeConstraint { get; init; } = typeConstraint;
    }

    public TypeParameterFragment Construct(ICompilationUnitContext compilationUnit) {
        return new TypeParameterFragment(Name.Resolve(compilationUnit), []);
    }
}

internal sealed class RuntimeSyntheticTypeParameter(Type typeParameterType) : ISyntheticTypeParameter, ISyntheticConstructable<TypeParameterFragment> {
    public SyntheticIdentifier Name { get; init; } = typeParameterType.Name;

    // Figure out constraints from base type and interfaces
    public sealed class Constraint(Type typeConstraint) : ISyntheticTypeParameter.IConstraint { }

    public TypeParameterFragment Construct(ICompilationUnitContext compilationUnit) {
        return new TypeParameterFragment();
    }
}