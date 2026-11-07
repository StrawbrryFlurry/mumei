using System.Text;

namespace Mumei.CodeGen;

[Flags]
public enum Modifiers {
    None = 0,
    Public = 1 << 0,
    Private = 1 << 1,
    Protected = 1 << 2,
    Internal = 1 << 3,
    File = 1 << 4,
    New = 1 << 5,
    Static = 1 << 6,
    Const = 1 << 7,
    Extern = 1 << 8,
    Abstract = 1 << 9,
    Virtual = 1 << 10,
    Override = 1 << 11,
    Sealed = 1 << 12,
    Readonly = 1 << 13,
    Volatile = 1 << 14,
    Unsafe = 1 << 15,
    Required = 1 << 16,
    Async = 1 << 17,
    Partial = 1 << 18,
    Ref = 1 << 19,

    Accessibility = Public | Private | Protected | Internal | File
}

[Flags]
public enum ParameterModifiers {
    None = 0,
    This = 1 << 0,
    Scoped = 1 << 1,
    Ref = 1 << 2,
    Out = 1 << 3,
    In = 1 << 4,
    Readonly = 1 << 5,
    Params = 1 << 6
}

public static class ModifierExtensions {
    private static readonly (Modifiers Flag, string Keyword)[] ModifierOrder = [
        (Modifiers.Public, "public"),
        (Modifiers.Private, "private"),
        (Modifiers.Protected, "protected"),
        (Modifiers.Internal, "internal"),
        (Modifiers.File, "file"),
        (Modifiers.New, "new"),
        (Modifiers.Static, "static"),
        (Modifiers.Const, "const"),
        (Modifiers.Extern, "extern"),
        (Modifiers.Abstract, "abstract"),
        (Modifiers.Virtual, "virtual"),
        (Modifiers.Sealed, "sealed"),
        (Modifiers.Override, "override"),
        (Modifiers.Readonly, "readonly"),
        (Modifiers.Volatile, "volatile"),
        (Modifiers.Unsafe, "unsafe"),
        (Modifiers.Required, "required"),
        (Modifiers.Async, "async"),
        (Modifiers.Ref, "ref"),
        (Modifiers.Partial, "partial")
    ];

    private static readonly (ParameterModifiers Flag, string Keyword)[] ParameterModifierOrder = [
        (ParameterModifiers.This, "this"),
        (ParameterModifiers.Scoped, "scoped"),
        (ParameterModifiers.Params, "params"),
        (ParameterModifiers.Ref, "ref"),
        (ParameterModifiers.Out, "out"),
        (ParameterModifiers.In, "in"),
        (ParameterModifiers.Readonly, "readonly")
    ];

    /// <summary>Renders the modifiers in canonical C# order, e.g. <c>"public static partial"</c>.</summary>
    public static string ToSyntax(this Modifiers modifiers) {
        var sb = new StringBuilder();
        foreach (var (flag, keyword) in ModifierOrder) {
            if ((modifiers & flag) == 0) {
                continue;
            }

            if (sb.Length > 0) {
                sb.Append(' ');
            }

            sb.Append(keyword);
        }

        return sb.ToString();
    }

    public static string ToSyntax(this ParameterModifiers modifiers) {
        var sb = new StringBuilder();
        foreach (var (flag, keyword) in ParameterModifierOrder) {
            if ((modifiers & flag) == 0) {
                continue;
            }

            if (sb.Length > 0) {
                sb.Append(' ');
            }

            sb.Append(keyword);
        }

        return sb.ToString();
    }

    /// <summary>The keyword to pass an argument for a parameter with these modifiers, e.g. <c>"ref "</c>.</summary>
    internal static string ToArgumentPrefix(this ParameterModifiers modifiers) {
        if ((modifiers & ParameterModifiers.Ref) != 0) {
            return "ref ";
        }

        if ((modifiers & ParameterModifiers.Out) != 0) {
            return "out ";
        }

        return (modifiers & ParameterModifiers.In) != 0 ? "in " : "";
    }
}
