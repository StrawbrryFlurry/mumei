namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticNamespace {
    public ISyntheticNamespace WithMember(ISyntheticMember member);
}

internal sealed class QtSyntheticNamespace(string name) : ISyntheticNamespace {
    public string Name { get; } = name;

    private List<ISyntheticMember>? _members;

    public ISyntheticNamespace WithMember(ISyntheticMember member) {
        _members ??= new List<ISyntheticMember>();
        _members.Add(member);
        return this;
    }
}