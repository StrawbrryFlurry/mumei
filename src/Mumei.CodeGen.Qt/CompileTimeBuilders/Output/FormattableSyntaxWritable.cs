using System.Runtime.CompilerServices;
using Mumei.CodeGen.Playground;

namespace Mumei.CodeGen.Qt.Output;

[InterpolatedStringHandler]
public ref struct FormattableSyntaxWritable {
    private ValueSyntaxWriter _writer;

    public int Length => _writer.Length;

    public FormattableSyntaxWritable(int literalLength, int formattedCount) {
        _writer = new ValueSyntaxWriter();
    }

    public void AppendLiteral(string s) {
        _writer.Write(s);
    }

    public void AppendFormatted(string s, string? format = null) {
        if (format == "q") {
            _writer.Write("\"");
        }

        _writer.Write(s);

        if (format == "q") {
            _writer.Write("\"");
        }
    }

    public void AppendFormatted<TRepresentable>(in TRepresentable representable) where TRepresentable : ISyntaxRepresentable {
        representable.WriteSyntax(ref _writer);
    }

    public void AppendFormatted<TRepresentable>(in TRepresentable representable, string format) where TRepresentable : ISyntaxRepresentable {
        representable.WriteSyntax(ref _writer, format);
    }

    public void AppendFormatted(Type type, string? format = null) {
        RuntimeTypeSerializer.SerializeInto(ref _writer, type, format);
    }

    public void AppendFormatted(AccessModifier modifier) {
        _writer.Write(modifier.AsCSharpString());
    }

    public void AppendFormatted(ParameterAttributes attributes) {
        if (attributes == ParameterAttributes.None) {
            return;
        }

        if (attributes.HasFlag(ParameterAttributes.This)) {
            _writer.Write("this ");
        }

        if (attributes.HasFlag(ParameterAttributes.Readonly)) {
            _writer.Write("readonly ");
        }

        _writer.Write(attributes switch {
            ParameterAttributes.In => "in ",
            ParameterAttributes.Out => "out ",
            ParameterAttributes.Ref => "ref ",
            ParameterAttributes.Params => "params ",
            _ => ""
        });
    }

    public readonly void CopyToAndDispose<TWriter>(ref TWriter writer) where TWriter : ISyntaxWriter {
        _writer.CopyTo(ref writer);
        _writer.Dispose();
    }

    public readonly void CopyToAndDispose(Span<char> buffer) {
        _writer.CopyTo(buffer);
        _writer.Dispose();
    }
}