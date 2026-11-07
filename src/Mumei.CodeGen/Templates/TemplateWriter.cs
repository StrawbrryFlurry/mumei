using System.ComponentModel;
using System.Text;
using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen;

/// <summary>Used by code generated for templates to assemble emitted code. Not intended to be used directly.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TemplateWriter {
    private readonly StringBuilder _text = new();

    public TemplateWriter Text(string text) {
        _text.Append(text);
        return this;
    }

    /// <summary>Inserts a value evaluated while generating: strings, numbers and enums as literals, types as their name.</summary>
    public TemplateWriter Value<T>(T value) {
        _text.Append(Format(value));
        return this;
    }

    /// <summary>Inserts a value into an interpolated string <c>$"…"</c>, replacing the hole <c>{value}</c>.</summary>
    public TemplateWriter InterpolatedValue<T>(T value) {
        if (value is string s) {
            var literal = CSharpLiteral.String(s);
            _text.Append(literal[1..^1].Replace("{", "{{").Replace("}", "}}"));
            return this;
        }

        _text.Append('{').Append(Format(value)).Append('}');
        return this;
    }

    public TemplateWriter Code(Code code) {
        _text.Append(code.ToString());
        return this;
    }

    public Code ToCode() {
        return _text.ToString();
    }

    public override string ToString() {
        return _text.ToString();
    }

    public static Code Build(Action<TemplateWriter> write) {
        var writer = new TemplateWriter();
        write(writer);
        return writer.ToCode();
    }

    private static string Format<T>(T value) {
        return value switch {
            null or string or char or bool or Enum or System.Type => CSharpLiteral.Format(value),
            IRenderFragment or CodeGen.Code => CodeGen.Code.Format(value, null),
            _ when value.GetType().IsPrimitive || value is decimal => CSharpLiteral.Format(value),
            _ => CodeGen.Code.Format(value, null)
        };
    }
}
