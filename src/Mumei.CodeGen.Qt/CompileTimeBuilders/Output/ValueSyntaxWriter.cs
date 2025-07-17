using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mumei.CodeGen.Qt.Output;

// If only we had allows ref struct (sigh)
public unsafe struct ValueSyntaxWriter : ISyntaxWriter {
    public const int StackBufferSize = 256;

    internal const char IndentChar = SyntaxWriter.IndentChar;
    internal const int IndentSpacing = SyntaxWriter.IndentSpacing;
    internal const string NewLine = SyntaxWriter.NewLine;

    public int Length => _bufferPosition;

    private string _indentString = "";
    private int _indentLevel;

    private char* _buffer;
    private int _bufferLength;
    private int _bufferPosition;
    private char[]? _rentedBuffer;

    private bool _requiresIndent = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueSyntaxWriter(char* initialBuffer, int initialBufferLength) {
        _buffer = initialBuffer;
        _bufferLength = initialBufferLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueSyntaxWriter(in Span<char> initialBuffer) {
        _buffer = (char*)Unsafe.AsPointer(ref initialBuffer.GetPinnableReference());
        _bufferLength = initialBuffer.Length;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Indent() {
        _indentLevel++;
        SyntaxWriter.RecalculateIndent(ref _indentString, _indentLevel);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dedent() {
        _indentLevel--;
        SyntaxWriter.RecalculateIndent(ref _indentString, _indentLevel);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(in ReadOnlySpan<char> text) {
        if (TryWriteIndent(text.Length)) {
            WriteCoreUnsafe(text);
            return;
        }

        WriteCore(text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(StringBuilder builder) {
        var s = builder.ToString();
        if (TryWriteIndent(s.Length)) {
            WriteCoreUnsafe(s);
            return;
        }

        WriteCore(s);
    }

    public void WriteLiteral<T>(T literal) where T : notnull {
        // We can only use the SpanFormattable implementation for <T>
        // in certain runtimes. We could duplicate the implementation based
        // on the runtime here, but the DefaultInterpolatedStringHandler does
        // essentially the same thing, albeit with a slight overhead since
        // it allocates its own buffer.
        var b = new DefaultInterpolatedStringHandler();
        b.AppendFormatted(literal);
        if (TryWriteIndent(b.Text.Length)) {
            WriteCoreUnsafe(b.Text);
            return;
        }

        WriteCore(b.Text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLine(string line) {
        if (TryWriteIndent(line.Length + NewLine.Length)) {
            WriteCoreUnsafe(line);
            WriteCoreUnsafe(NewLine);
        }
        else {
            WriteCore(line, NewLine.Length);
            WriteCoreUnsafe(NewLine);
        }

        _requiresIndent = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLine() {
        if (TryWriteIndent(NewLine.Length)) {
            WriteCoreUnsafe(NewLine);
        }
        else {
            WriteCore(NewLine);
        }

        _requiresIndent = true;
    }

    public void WriteFormatted(in FormattableSyntaxWritable writable) {
        var length = writable.Length;
        if (!TryWriteIndent(length)) {
            EnsureBufferCapacity(length);
        }

        var buffer = new Span<char>(_buffer, _bufferLength)[_bufferPosition..];
        writable.CopyToAndDispose(buffer);
        _bufferPosition += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteCore(in ReadOnlySpan<char> text, int requiresAdditionalCapacity = 0) {
        if (text.IsEmpty && requiresAdditionalCapacity == 0) {
            return;
        }

        EnsureBufferCapacity(text.Length + requiresAdditionalCapacity);
        WriteCoreUnsafe(text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteCoreUnsafe(in ReadOnlySpan<char> text) {
        var buffer = new Span<char>(_buffer, _bufferLength)[_bufferPosition..];
        text.CopyTo(buffer);
        _bufferPosition += text.Length;
    }

    private void EnsureBufferCapacity(int requiredLength) {
        requiredLength = _requiresIndent ? _indentString.Length + requiredLength : requiredLength;
        var currentLength = _bufferLength;
        requiredLength = _bufferPosition + requiredLength;
        if ((uint)requiredLength < (uint)currentLength) {
            return;
        }

        var newLength = Math.Max(currentLength == 0 ? 4 : currentLength * 2, requiredLength + 1);
        var newBuffer = ArrayPool<char>.Shared.Rent(newLength);
        var currentBuffer = new Span<char>(_buffer, currentLength);
        currentBuffer.CopyTo(newBuffer);
        if (_rentedBuffer != null) {
            ArrayPool<char>.Shared.Return(_rentedBuffer);
        }

        _rentedBuffer = newBuffer;
        var newBufferSpan = new Span<char>(newBuffer);
        _buffer = (char*)Unsafe.AsPointer(ref newBufferSpan.GetPinnableReference());
        _bufferLength = newLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<TRepresentable>(in TRepresentable representable, string? format = null) where TRepresentable : ISyntaxRepresentable {
        TryWriteIndent(0);
        representable.WriteSyntax(ref this, format);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo<TSyntaxWriter>(ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        var buffer = new Span<char>(_buffer, _bufferLength)[.._bufferPosition];
        writer.Write(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(Span<char> buffer) {
        var thisBuffer = new Span<char>(_buffer, _bufferLength)[.._bufferPosition];
        thisBuffer.CopyTo(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToSyntax() {
        var s = new string(_buffer, 0, _bufferPosition);
        Dispose();
        return s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() {
        return ToSyntax();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryWriteIndent(int additionalLineLength) {
        if (_requiresIndent) {
            WriteCore(_indentString.AsSpan(), additionalLineLength);
            _requiresIndent = false;
            return true;
        }

        return false;
    }

    public void Dispose() {
        if (_rentedBuffer is not null) {
            ArrayPool<char>.Shared.Return(_rentedBuffer);
        }

        this = default;
    }
}