namespace Mumei.CodeGen.Components;

internal sealed class ConstantSyntheticIdentifier(string name) : ISyntheticIdentifier, IEquatable<ConstantSyntheticIdentifier> {
    private readonly string _name = name;

    public string Resolve(IIdentifierResolver resolver) {
        return _name;
    }

    public bool Equals(ConstantSyntheticIdentifier? other) {
        return other != null && _name == other._name;
    }

    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || obj is ConstantSyntheticIdentifier other && Equals(other);
    }
    public override int GetHashCode() {
        return _name.GetHashCode();
    }
}