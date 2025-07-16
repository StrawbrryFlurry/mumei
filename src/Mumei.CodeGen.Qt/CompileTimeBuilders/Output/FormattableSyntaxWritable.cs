using System.Runtime.CompilerServices;
using Mumei.CodeGen.Playground;

namespace Mumei.CodeGen.Qt.Output;

[InterpolatedStringHandler]
public ref struct FormattableSyntaxWritable {
    private ValueSyntaxWriter _writer;

    public int Length => _writer.Length;

    public FormattableSyntaxWritable(int literalLength, int formattedCount, [CallerMemberName] string memberName = "") {
        _writer = new ValueSyntaxWriter();
    }

    public void AppendLiteral(string s) {
        _writer.Write(s);
    }

    public void AppendFormatted(string s) {
        _writer.Write(s);
    }

    public void AppendFormatted<TRepresentable>(TRepresentable representable) where TRepresentable : ISyntaxRepresentable {
        representable.WriteSyntax(ref _writer);
    }

    public void AppendFormatted<TRepresentable>(TRepresentable representable, string format) where TRepresentable : ISyntaxRepresentable {
        representable.WriteSyntax(ref _writer, format);
    }

    public void AppendFormatted(Type type, string? format = null) {
        RuntimeTypeSerializer.SerializeInto(ref _writer, type, format);
    }

    public void AppendFormatted(AccessModifier modifier) {
        _writer.Write(modifier.AsCSharpString());
    }

    public void AppendFormatted(ParameterModifier parameterModifier) {
        if (parameterModifier == ParameterModifier.None) {
            return;
        }

        if (parameterModifier.HasFlag(ParameterModifier.This)) {
            _writer.Write("this ");
        }

        _writer.Write(parameterModifier switch {
            ParameterModifier.In => "in ",
            ParameterModifier.Out => "out ",
            ParameterModifier.Ref => "ref ",
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