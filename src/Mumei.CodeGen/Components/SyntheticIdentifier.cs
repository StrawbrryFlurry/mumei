using Microsoft.Interop;

namespace Mumei.CodeGen.Components;

public abstract class SyntheticIdentifier : IEquatable<SyntheticIdentifier> {
    public abstract bool IsPartialDefinition { get; }

    /// <summary>
    /// The entire identifier name if it is known at instantiation time,
    /// or it's constant portion if it's a unique identifier.
    /// </summary>
    public abstract string ConstantValue { get; }

    public SyntheticIdentifier Join(SyntheticIdentifier other) {
        if (this is ConstantSyntheticIdentifier thisConstant && other is ConstantSyntheticIdentifier otherConstant) {
            return Constant(thisConstant.ConstantValue + otherConstant.ConstantValue);
        }

        return new ConcatenatedSyntheticIdentifier(this, other);
    }

    public abstract string Resolve(ISyntheticIdentifierScopeProvider scopeProvider);

    public static SyntheticIdentifier Constant(string value) {
        return new ConstantSyntheticIdentifier(value);
    }

    public static SyntheticIdentifier Unique(ISyntheticDeclaration declarationScope, string value) {
        return new UniqueSyntheticIdentifier(declarationScope, value);
    }

    public static SyntheticIdentifier operator +(SyntheticIdentifier left, SyntheticIdentifier right) {
        return left.Join(right);
    }

    public static implicit operator SyntheticIdentifier(string value) {
        return Constant(value);
    }

    public override string ToString() { return ConstantValue; }

    public virtual bool Equals(SyntheticIdentifier? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return IsPartialDefinition == other.IsPartialDefinition && ConstantValue == other.ConstantValue;
    }

    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((SyntheticIdentifier) obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(IsPartialDefinition, ConstantValue);
    }
}

internal sealed class UniqueSyntheticIdentifier(ISyntheticDeclaration declarationScope, string value) : SyntheticIdentifier {
    public override bool IsPartialDefinition => true;
    public override string ConstantValue { get; } = value;

    public override string Resolve(ISyntheticIdentifierScopeProvider scopeProvider) {
        var scope = scopeProvider.GetDeclarationScope(declarationScope);
        return scope.MakeUnique(ConstantValue);
    }

    public override int GetHashCode() {
        return HashCode.Combine(declarationScope, ConstantValue);
    }
}

internal sealed class ConstantSyntheticIdentifier(
    string value
) : SyntheticIdentifier {
    public override bool IsPartialDefinition => false;
    public override string ConstantValue { get; } = value;

    public override string Resolve(ISyntheticIdentifierScopeProvider scopeProvider) {
        return ConstantValue;
    }
}

internal sealed class ConcatenatedSyntheticIdentifier(
    SyntheticIdentifier left,
    SyntheticIdentifier right
) : SyntheticIdentifier {
    public override bool IsPartialDefinition { get; } = left.IsPartialDefinition || right.IsPartialDefinition;
    public override string ConstantValue { get; } = left.ConstantValue + right.ConstantValue;

    public SyntheticIdentifier Left { get; } = left;
    public SyntheticIdentifier Right { get; } = right;

    public override string Resolve(ISyntheticIdentifierScopeProvider scopeProvider) {
        var leftResolved = Left.Resolve(scopeProvider);
        var rightResolved = Right.Resolve(scopeProvider);
        return leftResolved + rightResolved;
    }
}