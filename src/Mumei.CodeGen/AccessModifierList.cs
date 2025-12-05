using System.Runtime.InteropServices;

namespace Mumei.CodeGen;

public readonly struct AccessModifierList {
    public static AccessModifierList Empty => new([]);
    private readonly ImmutableArray<AccessModifier> _modifiers;

    public ImmutableArray<AccessModifier> Modifiers => _modifiers;

    public static AccessModifierList operator +(AccessModifierList left, AccessModifierList right) {
        if (left.IsEmpty) {
            return right;
        }

        if (right.IsEmpty) {
            return left;
        }

        if (left.Contains(right)) {
            return left;
        }

        if (right.Contains(left)) {
            return right;
        }

        return new AccessModifierList([..left._modifiers, ..right._modifiers]);
    }

    public bool IsAbstract => Contains(AccessModifier.Abstract);

    public bool Contains(AccessModifierList modifier) {
        return modifier._modifiers.AsSpan().Overlaps(_modifiers.AsSpan());
    }

    internal AccessModifierList(AccessModifier modifier) {
        _modifiers = [modifier];
    }

    internal AccessModifierList(ReadOnlySpan<AccessModifier> modifiers) {
        // TODO: Cache all common combinations
        _modifiers = modifiers.ToImmutableArray();
        var arr = ImmutableCollectionsMarshal.AsArray(_modifiers);
        Array.Sort(arr);
    }

    public AccessModifierList Extend(AccessModifier modifier) {
        if (Contains(modifier)) {
            return this;
        }

        return new AccessModifierList([.._modifiers, modifier]);
    }

    public bool Is(string modifier) {
        return _modifiers.Length == 1 && _modifiers[0] == modifier;
    }

    public bool Is(AccessModifierList modifier) {
        return _modifiers.Length == 1 && modifier._modifiers.Length == 1 && _modifiers[0] == modifier._modifiers[0];
    }

    public bool IsEmpty => _modifiers.IsDefaultOrEmpty;

    public ImmutableArray<AccessModifier>.Enumerator GetEnumerator() {
        return _modifiers.GetEnumerator();
    }

    public string AsCSharpString() {
        return ToString();
    }

    public override string ToString() {
        return string.Join(" ", _modifiers);
    }
}

public readonly struct AccessModifier(string value) : IEquatable<AccessModifier>, IComparable<AccessModifier> {
    public static readonly AccessModifier Public = new("public");
    public static readonly AccessModifier Private = new("private");
    public static readonly AccessModifier Abstract = new("abstract");
    public static readonly AccessModifier Protected = new("protected");
    public static readonly AccessModifier Internal = new("internal");
    public static readonly AccessModifier File = new("file");
    public static readonly AccessModifier Sealed = new("sealed");
    public static readonly AccessModifier Readonly = new("readonly");
    public static readonly AccessModifier Static = new("static");
    public static readonly AccessModifier Partial = new("partial");
    public static readonly AccessModifier Virtual = new("virtual");
    public static readonly AccessModifier Override = new("override");
    public static readonly AccessModifier Async = new("async");

    public readonly string Value = value;

    internal bool HasValue => !string.IsNullOrEmpty(Value);

    public static implicit operator AccessModifier(string accessModifier) {
        return new AccessModifier(accessModifier);
    }

    public static implicit operator AccessModifierList(AccessModifier accessModifier) {
        return new AccessModifierList(accessModifier);
    }

    public static AccessModifierList operator +(AccessModifier left, AccessModifier right) {
        if (left == right) {
            return new AccessModifierList(left);
        }

        if (!left.HasValue) {
            return new AccessModifierList(right);
        }

        if (!right.HasValue) {
            return new AccessModifierList(left);
        }

        return new AccessModifierList([left, right]);
    }

    public static AccessModifierList operator +(AccessModifier left, AccessModifierList right) {
        if (!left.HasValue) {
            return right;
        }

        if (right.IsEmpty) {
            return new AccessModifierList(left);
        }

        return right.Extend(left);
    }

    public static AccessModifierList operator +(AccessModifierList left, AccessModifier right) {
        if (!right.HasValue) {
            return left;
        }

        if (left.IsEmpty) {
            return new AccessModifierList(right);
        }

        return left.Extend(right);
    }

    public static bool operator ==(AccessModifier left, AccessModifier right) {
        return left.Equals(right);
    }

    public static bool operator !=(AccessModifier left, AccessModifier right) {
        return !left.Equals(right);
    }

    public bool Equals(AccessModifier other) {
        return other.Value == Value;
    }

    private static readonly Dictionary<string, int> ModifierOrder = new() {
        ["public"] = 100,
        ["private"] = 101,
        ["protected"] = 102,
        ["internal"] = 103,
        ["file"] = 104,

        ["abstract"] = 200,
        ["virtual"] = 201,
        ["override"] = 202,
        ["sealed"] = 203,

        ["static"] = 300,
        ["readonly"] = 301,
        ["partial"] = 302
    };

    public int CompareTo(AccessModifier other) {
        if (string.IsNullOrEmpty(Value) && string.IsNullOrEmpty(other.Value)) {
            return 0;
        }
        if (string.IsNullOrEmpty(Value)) {
            return -1;
        }

        if (string.IsNullOrEmpty(other.Value)) {
            return 1;
        }

        var thisOrder = GetOrderValue(Value);
        var otherOrder = GetOrderValue(other.Value);

        return thisOrder.CompareTo(otherOrder);
    }

    private static int GetOrderValue(string modifier) {
        if (ModifierOrder.TryGetValue(modifier.ToLowerInvariant(), out var order)) {
            return order;
        }

        // Place unknown modifiers at the end
        return int.MaxValue;
    }

    public override string ToString() {
        return Value;
    }
}