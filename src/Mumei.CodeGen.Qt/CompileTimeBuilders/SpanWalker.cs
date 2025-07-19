using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Playground;

internal ref struct SpanWalker<T>(
    ReadOnlySpan<T> span
) {
    private readonly ReadOnlySpan<T> _span = span;
    private int _pos;

    public ReadOnlySpan<T> Span {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    } = span;

    public bool IsAtEnd => _pos == _span.Length;
}