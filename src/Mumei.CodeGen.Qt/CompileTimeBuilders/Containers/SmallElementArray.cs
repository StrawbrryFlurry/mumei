using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mumei.CodeGen.Qt.Containers;

[SkipLocalsInit]
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
public readonly struct SmallElementArray<T> {
    private readonly T _element0;
    private readonly T _element1;
    private readonly T _element2;
    private readonly T _element3;
    private readonly T _element4;
    private readonly T _element5;
    private readonly T _element6;
    private readonly T _element7;

    private readonly int _length;
    public int Length => _length;

    public static SmallElementArray<T> Empty => default;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallElementArray(
        T element1
    ) {
        _element0 = element1;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallElementArray(
        T element1,
        T element2
    ) {
        _element0 = element1;
        _element1 = element2;
        _length = 2;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallElementArray(
        T element1,
        T element2,
        T element3
    ) {
        _element0 = element1;
        _element1 = element2;
        _element2 = element3;
        _length = 3;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallElementArray(
        T element1,
        T element2,
        T element3,
        T element4
    ) {
        _element0 = element1;
        _element1 = element2;
        _element2 = element3;
        _element3 = element4;
        _length = 4;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallElementArray(
        T element1,
        T element2,
        T element3,
        T element4,
        T element5
    ) {
        _element0 = element1;
        _element1 = element2;
        _element2 = element3;
        _element3 = element4;
        _element4 = element5;
        _length = 5;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallElementArray(
        T element1,
        T element2,
        T element3,
        T element4,
        T element5,
        T element6
    ) {
        _element0 = element1;
        _element1 = element2;
        _element2 = element3;
        _element3 = element4;
        _element4 = element5;
        _element5 = element6;
        _length = 6;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallElementArray(
        T element1,
        T element2,
        T element3,
        T element4,
        T element5,
        T element6,
        T element7
    ) {
        _element0 = element1;
        _element1 = element2;
        _element2 = element3;
        _element3 = element4;
        _element4 = element5;
        _element5 = element6;
        _element6 = element7;
        _length = 7;
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SmallElementArray(
        T element1,
        T element2,
        T element3,
        T element4,
        T element5,
        T element6,
        T element7,
        T element8
    ) {
        _element0 = element1;
        _element1 = element2;
        _element2 = element3;
        _element3 = element4;
        _element4 = element5;
        _element5 = element6;
        _element6 = element7;
        _element7 = element8;
        _length = 8;
    }

    [UnscopedRef]
    public ref readonly T this[int index] {
        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (index < 0 || index >= _length) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.Add(ref Unsafe.AsRef(in _element0), index);
        }
    }
}