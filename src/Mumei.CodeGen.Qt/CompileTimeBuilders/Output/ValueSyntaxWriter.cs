﻿using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mumei.CodeGen.Qt.Output;

// If only we had allows ref struct (sigh)
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
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

#if NET6_0_OR_GREATER
    public void WriteLiteral<T>(T literal) where T : notnull {
        if (literal is string s) {
            WriteCore(s);
            return;
        }

        if (literal is char c) {
            WriteCore(new ReadOnlySpan<char>(ref c));
            return;
        }

        if (literal is ISpanFormattable sf) {
            TryWriteIndent(0);
            var buffer = new Span<char>(_buffer, _bufferLength)[_bufferPosition..];

            TryWriteLiteral:
            var success = sf.TryFormat(buffer, out var written, default, null);
            if (!success) {
                EnsureBufferCapacity(written);
                goto TryWriteLiteral;
            }

            _bufferPosition += written;
            return;
        }

        var str = literal.ToString();
        if (str is null) {
            TryWriteIndent(0);
            return;
        }

        if (TryWriteIndent(str.Length)) {
            WriteCoreUnsafe(str);
            return;
        }

        WriteCore(str);
    }
#else
    public void WriteLiteral<T>(T literal) where T : notnull {
        if (literal is string s) {
            WriteCore(s);
            return;
        }

        if (literal is char c) {
            WriteCore([c]);
            return;
        }

        var str = literal.ToString();
        if (TryWriteIndent(str.Length)) {
            WriteCoreUnsafe(str);
            return;
        }

        WriteCore(str);
    }
#endif

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

    private string DebuggerDisplay() {
        return new string(_buffer, 0, _bufferPosition);
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