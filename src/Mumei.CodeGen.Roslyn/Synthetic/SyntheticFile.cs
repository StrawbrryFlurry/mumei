namespace Mumei.CodeGen.Roslyn.Synthetic;

public sealed record SyntheticFile {
    public EquatableArray<SyntheticNamespace> Namespaces { get; init; }
    public EquatableArray<IGeneratorFeatureRequirement> FeatureRequirements { get; init; }
}