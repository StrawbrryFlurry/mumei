namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public readonly struct SyntheticAccessModifier {
    public static readonly SyntheticAccessModifier Public = new("public");
    public static readonly SyntheticAccessModifier Private = new("private");
    public static readonly SyntheticAccessModifier Protected = new("protected");
    public static readonly SyntheticAccessModifier Internal = new("internal");
    public static readonly SyntheticAccessModifier File = new("file");
    public static readonly SyntheticAccessModifier Sealed = new("sealed");
    public static readonly SyntheticAccessModifier Readonly = new("readonly");
    public static readonly SyntheticAccessModifier Static = new("static");

    private readonly string[] _modifierList;

    public static SyntheticAccessModifier operator +(SyntheticAccessModifier left, SyntheticAccessModifier right) {
        return new SyntheticAccessModifier([..left._modifierList, ..right._modifierList]);
    }

    private SyntheticAccessModifier(string modifier) {
        _modifierList = [modifier];
    }

    private SyntheticAccessModifier(string[] modifierList) {
        _modifierList = modifierList;
    }

    public override string ToString() {
        return string.Join(" ", _modifierList);
    }
}