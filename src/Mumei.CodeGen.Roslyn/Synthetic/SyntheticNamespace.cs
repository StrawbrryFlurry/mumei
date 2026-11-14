namespace Mumei.CodeGen.Roslyn.Synthetic;

public sealed record SyntheticNamespace {
    public string Name { get; init; }

    public bool IsGlobal => Name == "";
}