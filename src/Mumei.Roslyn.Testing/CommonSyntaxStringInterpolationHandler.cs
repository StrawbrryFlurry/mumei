using System.Runtime.CompilerServices;
using System.Text;
using Mumei.CodeGen;

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
    ///     "nq" - No qualification, just the type name. (default)
    ///     "typeof" or "t" - Wraps the type name in a typeof
    ///     "nq+typeof" or "typeof+nq" or "nq+t" or "t+nq" - No qualification, wrapped in typeof
    ///     "-*" - Removes the specified part from the type name. E.g. Name: Awesome.Namespace.Testing.AwesomeClass, Format: "Testing" => Awesome.Namespace.AwesomeClass
    ///     "global" or "g" - Fully qualified with global::
    ///     "-*+g" or "-*+global" - Removes the specified part and fully qualifies with global::
    /// </param>
    public void AppendFormatted(Type type, string? format = null) {
        string name;
        format ??= "";
        var options = format.Split("+");
        if (options.Contains("global") || options.Contains("g")) {
            name = RuntimeTypeSerializer.GetTypeFullName(type, true);
        } else if (format.Contains("nq")) {
            name = RuntimeTypeSerializer.GetTypeFullName(type, false);
        } else {
            name = RuntimeTypeSerializer.GetTypeFullName(type, false);
        }

        if (options.Contains("typeof") || options.Contains("t")) {
            name = $"typeof({name})";
        }

        var removeOptions = options.Where(o => o.StartsWith('-')).ToArray();
        foreach (var removeOption in removeOptions) {
            var toRemove = removeOption[1..];
            var replacement = "";

            if (toRemove.IndexOf(':') is var idx and > 0) {
                replacement = removeOption[(idx + 1)..];
                toRemove = toRemove[..idx];
            }

            name = name.Replace($".{toRemove}", replacement).Replace(toRemove, replacement);
        }

        _builder.Append(name);
    }

    public void AppendFormatted<T>(T value) {
        _builder.Append(value?.ToString());
    }

    public override string ToString() {
        return _builder.ToString();
    }
}