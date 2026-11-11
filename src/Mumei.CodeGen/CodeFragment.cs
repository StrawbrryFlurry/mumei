using System.Runtime.CompilerServices;
using System.Text;

namespace Mumei.CodeGen;

[InterpolatedStringHandler]
public readonly record struct CodeFragment {
    private readonly StringBuilder _builder;

    public CodeFragment(int literalLength, int formattedCount) {
        _builder = new StringBuilder(literalLength);
    }


    public void AppendLiteral(string s) {
        _builder.Append(s);
    }

    public void AppendFormatted(string s) {
        _builder.Append(s);
    }

    public void AppendFormatted(Type type, string? format = null) {
        string name;
        format ??= "";
        var options = format.Split("+");

        if (options.Contains("global") || options.Contains("g")) {
            name = RuntimeTypeSerializer.GetTypeFullName(type, true);
        }
        else if (options.Contains("qualified") || options.Contains("q")) {
            name = RuntimeTypeSerializer.GetTypeFullName(type, false);
        }
        else if (options.Contains("namespace") || options.Contains("ns")) {
            name = type.Namespace ?? "";
        }
        else {
            name = type.Name;
        }

        if (options.Contains("typeof")) {
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
        _builder.Append(value);
    }

    public override string ToString() {
        return _builder.ToString();
    }
}