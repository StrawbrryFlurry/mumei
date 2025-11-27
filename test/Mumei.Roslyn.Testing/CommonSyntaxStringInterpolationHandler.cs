using System.Runtime.CompilerServices;
using System.Text;

namespace Mumei.Roslyn.Testing;

[InterpolatedStringHandler]
public readonly ref struct CommonSyntaxStringInterpolationHandler {
    private readonly StringBuilder _builder;

    public CommonSyntaxStringInterpolationHandler(int literalLength, int formattedCount) {
        _builder = new StringBuilder(literalLength);
    }

    public void AppendLiteral(string s) {
        _builder.Append(s);
    }

    public void AppendFormatted(string s) {
        _builder.Append(s);
    }

    /// <summary>
    /// Appends the formatted type name according to the specified format.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="format">
    ///     "nq" - No qualification, just the type name.
    ///     "typeof" or "t" - Wraps the type name in a typeof
    ///     "nq+typeof" or "typeof+nq" or "nq+t" or "t+nq" - No qualification, wrapped in typeof
    /// </param>
    public void AppendFormatted(Type type, string? format = null) {
        var name = type.FullName ?? type.Name;

        var str = format switch {
            "nq" => name,
            "typeof" or "t" or "nq+typeof" or "typeof+nq" or "nq+t" or "t+nq" => $"typeof({name})",
            "typeof+g" or "t+g" or "g+typeof" or "g+t" => $"typeof(global::{name})",
            "g" or "global" => $"global::{name}",
            _ => name
        };

        _builder.Append(str);
    }

    public void AppendFormatted<T>(T value) {
        _builder.Append(value?.ToString());
    }

    public override string ToString() {
        return _builder.ToString();
    }
}