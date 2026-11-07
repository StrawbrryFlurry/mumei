using System.Globalization;
using System.Text;

namespace Mumei.CodeGen;

/// <summary>
/// Formats runtime values as C# literal expressions, e.g. <c>"a\"b"</c>, <c>1.5F</c> or <c>true</c>.
/// </summary>
public static class CSharpLiteral {
    public static bool CanFormat(object? value) {
        return value is null or string or char or bool or Enum or Type
            or sbyte or byte or short or ushort or int or uint or long or ulong
            or float or double or decimal;
    }

    public static string Format(object? value) {
        return value switch {
            null => "null",
            string s => String(s),
            char c => Char(c),
            bool b => b ? "true" : "false",
            Enum e => FormatEnum(e),
            Type t => $"typeof({RuntimeTypeSerializer.GetTypeFullName(t)})",
            int i => i.ToString(CultureInfo.InvariantCulture),
            uint u => u.ToString(CultureInfo.InvariantCulture) + "U",
            long l => l.ToString(CultureInfo.InvariantCulture) + "L",
            ulong ul => ul.ToString(CultureInfo.InvariantCulture) + "UL",
            sbyte sb => $"(sbyte){sb.ToString(CultureInfo.InvariantCulture)}",
            byte by => $"(byte){by.ToString(CultureInfo.InvariantCulture)}",
            short sh => $"(short){sh.ToString(CultureInfo.InvariantCulture)}",
            ushort us => $"(ushort){us.ToString(CultureInfo.InvariantCulture)}",
            float f => FormatFloat(f),
            double d => FormatDouble(d),
            decimal m => m.ToString(CultureInfo.InvariantCulture) + "M",
            _ => throw new ArgumentException(
                $"Values of type '{value.GetType()}' cannot be represented as a C# literal.",
                nameof(value)
            )
        };
    }

    public static string String(string value) {
        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
        foreach (var c in value) {
            AppendEscaped(sb, c, '"');
        }

        sb.Append('"');
        return sb.ToString();
    }

    public static string Char(char value) {
        var sb = new StringBuilder(4);
        sb.Append('\'');
        AppendEscaped(sb, value, '\'');
        sb.Append('\'');
        return sb.ToString();
    }

    private static void AppendEscaped(StringBuilder sb, char c, char quote) {
        switch (c) {
            case '\\': sb.Append(@"\\"); return;
            case '\0': sb.Append(@"\0"); return;
            case '\a': sb.Append(@"\a"); return;
            case '\b': sb.Append(@"\b"); return;
            case '\f': sb.Append(@"\f"); return;
            case '\n': sb.Append(@"\n"); return;
            case '\r': sb.Append(@"\r"); return;
            case '\t': sb.Append(@"\t"); return;
            case '\v': sb.Append(@"\v"); return;
        }

        if (c == quote) {
            sb.Append('\\').Append(c);
            return;
        }

        if (char.IsControl(c) || char.IsSurrogate(c) || c is '\u0085' or '\u2028' or '\u2029') {
            sb.Append(@"\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
            return;
        }

        sb.Append(c);
    }

    private static string FormatFloat(float f) {
        if (float.IsNaN(f)) {
            return "float.NaN";
        }

        if (float.IsPositiveInfinity(f)) {
            return "float.PositiveInfinity";
        }

        if (float.IsNegativeInfinity(f)) {
            return "float.NegativeInfinity";
        }

        return f.ToString("R", CultureInfo.InvariantCulture) + "F";
    }

    private static string FormatDouble(double d) {
        if (double.IsNaN(d)) {
            return "double.NaN";
        }

        if (double.IsPositiveInfinity(d)) {
            return "double.PositiveInfinity";
        }

        if (double.IsNegativeInfinity(d)) {
            return "double.NegativeInfinity";
        }

        return d.ToString("R", CultureInfo.InvariantCulture) + "D";
    }

    private static string FormatEnum(Enum e) {
        var type = e.GetType();
        var typeName = RuntimeTypeSerializer.GetTypeFullName(type);
        if (Enum.IsDefined(type, e)) {
            return $"{typeName}.{e}";
        }

        var underlying = Convert.ChangeType(e, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture);
        return $"(({typeName}){Format(underlying)})";
    }
}
