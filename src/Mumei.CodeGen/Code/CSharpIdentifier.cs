namespace Mumei.CodeGen;

public static class CSharpIdentifier {
    private static readonly HashSet<string> Keywords = [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
        "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
        "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    ];

    /// <summary>Prefixes reserved keywords with <c>@</c>, e.g. <c>class</c> becomes <c>@class</c>.</summary>
    public static string Escape(string identifier) {
        return Keywords.Contains(identifier) ? "@" + identifier : identifier;
    }

    /// <summary>Turns arbitrary text into a valid identifier, e.g. <c>"my-type.cs"</c> becomes <c>"my_type_cs"</c>.</summary>
    public static string Sanitize(string text) {
        if (text.Length == 0) {
            return "_";
        }

        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++) {
            if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_') {
                chars[i] = '_';
            }
        }

        var result = new string(chars);
        return char.IsDigit(result[0]) ? "_" + result : result;
    }
}
