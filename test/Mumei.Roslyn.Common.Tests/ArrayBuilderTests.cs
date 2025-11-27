namespace Mumei.Roslyn.Common.Tests;

public sealed class ArrayBuilderTests {
    [Fact]
    public void Ctor_Empty() {
        var builder = new ArrayBuilder<int>();
        Assert.Equal(0, builder.UnsafeBuffer.Length);
    }

    [Fact]
    public void Ctor_StackAlloc() {
        var builder = new ArrayBuilder<int>(stackalloc int[ArrayBuilder.InitSize]);
        Assert.Equal(ArrayBuilder.InitSize, builder.UnsafeBuffer.Length);
    }

    [Fact]
    public void Ctor_Array() {
        var builder = new ArrayBuilder<int>(new int[ArrayBuilder.InitSize]);
        Assert.Equal(ArrayBuilder.InitSize, builder.UnsafeBuffer.Length);
    }

    [Fact]
    public void Ctor_Default() {
        var builder = default(ArrayBuilder<int>);
        Assert.Equal(0, builder.UnsafeBuffer.Length);
    }

    [Fact]
    public void Add_InitialBufferIsEmpty_GrowsAndAddsElement() {
        var builder = new ArrayBuilder<int>();

        builder.Add(1);

        Assert.Equal(1, builder.Elements.Length);
    }

    [Fact]
    public void Add_InitialBufferRequiresGrowing_GrowsAndAddsElement() {
        var builder = new ArrayBuilder<int>(stackalloc int[4]);

        builder.AddRange([1, 2, 3, 4]);
        builder.Add(5);

        Assert.Equal([1, 2, 3, 4, 5], builder.Elements);
    }

    [Fact]
    public void Add_IsDisposed_ContinuesWithEmptyBuffer() {
        var builder = new ArrayBuilder<int>();

        builder.Dispose();
        builder.Add(1);

        Assert.Equal([1], builder.Elements);
    }

    [Fact]
    public void AddRange_SingleElement_InitialBufferIsEmpty_GrowsBufferAndAddsElement() {
        var builder = new ArrayBuilder<int>();

        builder.AddRange([1]);

        Assert.Equal([1], builder.Elements);
    }

    [Fact]
    public void AddRange_CountInGrowthRange_InitialBufferIsEmpty_GrowsBufferAndAddsElement() {
        var builder = new ArrayBuilder<int>();

        builder.AddRange([1, 2, 3, 4]);

        Assert.Equal([1, 2, 3, 4], builder.Elements);
    }

    [Fact]
    public void AddRange_CountExceedsDefaultGrowth_InitialBufferIsEmpty_GrowsBufferAndAddsElement() {
        var builder = new ArrayBuilder<int>();

        builder.AddRange([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);

        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16], builder.Elements);
    }

    [Fact]
    public void ToString_EmptyBuffer_ReturnsEmptyString() {
        var builder = new ArrayBuilder<char>();

        Assert.Equal(string.Empty, builder.ToStringAndFree());
    }

    [Fact]
    public void ToString_NonEmptyBuffer_ReturnsConcatenatedString() {
        var builder = new ArrayBuilder<char>(stackalloc char[4]);

        builder.AddRange("Mumei");
        builder.Add(' ');
        builder.AddRange("Nanashi");

        Assert.Equal("Mumei Nanashi", builder.ToStringAndFree());
    }

    [Fact]
    public void Dispose_FreesBuffer() {
        var builder = new ArrayBuilder<int>();

        builder.Add(1);
        builder.Dispose();

        Assert.Equal([], builder.Elements);
        Assert.Equal([], builder.UnsafeBuffer);
    }

    [Fact]
    public void Dispose_StackAllocBuffer_DoesNotThrow() {
        var builder = new ArrayBuilder<int>(stackalloc int[ArrayBuilder.InitSize]);

        builder.Add(1);
        builder.Dispose();

        Assert.Equal([], builder.Elements);
        Assert.Equal([], builder.UnsafeBuffer);
    }

    [Fact]
    public void Dispose_AlreadyDisposed_DoesNotThrow() {
        var builder = new ArrayBuilder<int>();

        builder.Dispose();

        // Should not throw
        builder.Dispose();

        Assert.Equal([], builder.Elements);
        Assert.Equal([], builder.UnsafeBuffer);
    }

    [Fact]
    public void ToArray_EmptyBuffer_ReturnsEmptyArray() {
        var builder = new ArrayBuilder<int>();
        var array = builder.ToArrayAndFree();
        Assert.Empty(array);
    }

    [Fact]
    public void ToArray_NonEmptyBuffer_ReturnsArrayWithElements() {
        var builder = new ArrayBuilder<int>();
        builder.AddRange([1, 2, 3, 4]);

        var array = builder.ToArrayAndFree();
        Assert.Equal([1, 2, 3, 4], array);
    }

    [Fact]
    public void ToArray_FreesBuffer_WhenBufferIsNotEmpty() {
        var builder = new ArrayBuilder<int>();
        builder.AddRange([1, 2, 3, 4]);

        _ = builder.ToArrayAndFree();
        Assert.Equal([], builder.UnsafeBuffer);
    }
}