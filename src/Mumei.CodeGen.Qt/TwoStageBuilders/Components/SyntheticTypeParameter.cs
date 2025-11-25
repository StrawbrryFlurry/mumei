using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticTypeParameter(string name) : ISyntheticTypeParameter {
    public string Name { get; init; } = name;

    public sealed class Constraint(ISyntheticType typeConstraint) : ISyntheticTypeParameter.IConstraint {
        public ISyntheticType TypeConstraint { get; init; } = typeConstraint;
    }
}

internal sealed class RuntimeSyntheticTypeParameter(Type typeParameterType) : ISyntheticTypeParameter {
    public string Name { get; init; } = typeParameterType.Name;

    public sealed class Constraint(Type typeConstraint) : ISyntheticTypeParameter.IConstraint {

    }
}

internal sealed class RoslynSyntheticTypeParameter(ITypeParameterSymbol parameter) : ISyntheticTypeParameter {
    public string Name { get; init; } = parameter.Name;

    public sealed class Constraint(ISyntheticType typeConstraint) : ISyntheticTypeParameter.IConstraint {
        public ISyntheticType TypeConstraint { get; init; } = typeConstraint;
    }
}