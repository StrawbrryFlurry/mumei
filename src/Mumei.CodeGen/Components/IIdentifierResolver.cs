namespace Mumei.CodeGen.Components;

public interface IIdentifierResolver {
    public string MakeUnique(IdentifierScope scope, string name);
}

public readonly struct IdentifierScope : IEquatable<IdentifierScope> {
    internal int ScopeId { get; }

    internal static IdentifierScope Global { get; } = new(0);

    internal IdentifierScope(int id) {
        ScopeId = id;
    }

    public bool Equals(IdentifierScope other) {
        // We rely on the scope id being unique across the entire resolver lifetime
        // so we don't have to check parent ids here.
        return ScopeId == other.ScopeId;
    }

    public override bool Equals(object? obj) {
        return obj is IdentifierScope other && Equals(other);
    }

    public override int GetHashCode() {
        return ScopeId;
    }
}