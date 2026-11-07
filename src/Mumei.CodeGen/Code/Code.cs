using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen;

/// <summary>
/// A snippet of C# code, usually written as an interpolated string. Holes are rendered as code:
/// <list type="bullet">
///   <item><see cref="TypeRef"/>, models, member builders and other <see cref="IRenderFragment"/>s render as themselves
///   (types as their global name, members as their name)</item>
///   <item><see cref="Type"/> renders as its global name; <c>{typeof(X):typeof}</c> renders <c>typeof(global::X)</c></item>
///   <item><c>string</c> is inserted verbatim; use <c>{value:literal}</c> to insert any value as an escaped C# literal</item>
///   <item>numbers, <c>bool</c> and enums render as C# literals</item>
/// </list>
/// </summary>
[InterpolatedStringHandler]
public readonly struct Code : IRenderFragment, IEquatable<Code> {
    private const string LiteralFormat = "literal";
    private const string TypeOfFormat = "typeof";

    private readonly StringBuilder? _builder;
    private readonly string? _text;

    public Code(int literalLength, int formattedCount) {
        _builder = new StringBuilder(literalLength + formattedCount * 16);
    }

    private Code(string text) {
        _text = text;
    }

    public static Code Empty => default;

    public bool IsEmpty => (_text?.Length ?? _builder?.Length ?? 0) == 0;

    public void AppendLiteral(string value) {
        _builder!.Append(value);
    }

    public void AppendFormatted(string? value, string? format = null) {
        if (format == LiteralFormat) {
            _builder!.Append(value is null ? "null" : CSharpLiteral.String(value));
            return;
        }

        _builder!.Append(value);
    }

    public void AppendFormatted(Type type, string? format = null) {
        if (format == LiteralFormat || format == TypeOfFormat) {
            _builder!.Append(CSharpLiteral.Format(type));
            return;
        }

        _builder!.Append(RuntimeTypeSerializer.GetTypeFullName(type));
    }

    public void AppendFormatted(Code code) {
        _builder!.Append(code.ToString());
    }

    public void AppendFormatted<T>(T value, string? format = null) {
        _builder!.Append(Format(value, format));
    }

    public static implicit operator Code(string? text) {
        return new Code(text ?? "");
    }

    /// <summary>Renders any value as an escaped C# literal, e.g. <c>"a\"b"</c> or <c>1.5F</c>.</summary>
    public static Code Literal(object? value) {
        return new Code(CSharpLiteral.Format(value));
    }

    /// <summary>Renders each item using the hole rules and joins them, e.g. <c>Code.Join(", ", type.Properties)</c>.</summary>
    public static Code Join<T>(string separator, IEnumerable<T> items) {
        return new Code(string.Join(separator, items.Select(x => Format(x, null))));
    }

    public static Code Join<T>(string separator, IEnumerable<T> items, Func<T, Code> selector) {
        return new Code(string.Join(separator, items.Select(x => selector(x).ToString())));
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(ToString());
    }

    public override string ToString() {
        return _text ?? _builder?.ToString() ?? "";
    }

    public bool Equals(Code other) {
        return ToString() == other.ToString();
    }

    public override bool Equals(object? obj) {
        return obj is Code other && Equals(other);
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    internal static string Format<T>(T value, string? format) {
        if (format == LiteralFormat) {
            return CSharpLiteral.Format(value);
        }

        switch (value) {
            case null:
                return "";
            case string s:
                return s;
            case Code code:
                return code.ToString();
            case TypeRef type:
                return format == TypeOfFormat ? $"typeof({type.FullName})" : type.FullName;
            case Type type:
                return format == TypeOfFormat ? CSharpLiteral.Format(type) : RuntimeTypeSerializer.GetTypeFullName(type);
            case bool or Enum:
                return CSharpLiteral.Format(value);
            case char c:
                return c.ToString();
            case IRenderFragment fragment:
                return RenderFragment(fragment);
            case IFormattable formattable:
                return formattable.ToString(format, CultureInfo.InvariantCulture);
            default:
                return value.ToString() ?? "";
        }
    }

    private static string RenderFragment(IRenderFragment fragment) {
        using var tree = new SourceFileRenderTreeBuilder();
        tree.Node(fragment);
        return tree.GetSourceText();
    }
}
