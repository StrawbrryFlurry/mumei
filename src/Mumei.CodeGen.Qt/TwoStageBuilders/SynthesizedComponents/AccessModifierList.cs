namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct AccessModifierList {
    public static AccessModifierList Empty => new([]);

    public static readonly AccessModifierList Public = new("public");
    public static readonly AccessModifierList Private = new("private");
    public static readonly AccessModifierList Abstract = new("abstract");
    public static readonly AccessModifierList Protected = new("protected");
    public static readonly AccessModifierList Internal = new("internal");
    public static readonly AccessModifierList File = new("file");
    public static readonly AccessModifierList Sealed = new("sealed");
    public static readonly AccessModifierList Readonly = new("readonly");
    public static readonly AccessModifierList Static = new("static");

    private static readonly AccessModifierList PublicStatic = Public + Static;

    private readonly string[] _modifiers;

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

        if (left.Is(Public) && right.Is(Static)) {
            return PublicStatic;
        }

        return new AccessModifierList([..left._modifiers, ..right._modifiers]);
    }

    public bool IsAbstract => Contains(Abstract);

    public bool Contains(AccessModifierList modifier) {
        return modifier._modifiers.AsSpan().Overlaps(_modifiers.AsSpan());
    }

    private AccessModifierList(string modifier) {
        _modifiers = [modifier];
    }

    private AccessModifierList(string[] modifiers) {
        _modifiers = modifiers;
    }

    public bool Is(string modifier) {
        return _modifiers.Length == 1 && _modifiers[0] == modifier;
    }

    public bool Is(AccessModifierList modifier) {
        return _modifiers.Length == 1 && modifier._modifiers.Length == 1 && _modifiers[0] == modifier._modifiers[0];
    }

    public bool IsEmpty => _modifiers.Length == 0;

    public string AsCSharpString() {
        return ToString();
    }

    public override string ToString() {
        return string.Join(" ", _modifiers);
    }
}