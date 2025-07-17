using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Tests;

public sealed class ValueSyntaxWriterTests {
    [Fact]
    public void Empty() {
        var vsw = new ValueSyntaxWriter();
        Assert.Equal("", vsw.ToString());
    }

    [Fact]
    public void Default() {
        var vsw = default(ValueSyntaxWriter);
        Assert.Equal("", vsw.ToString());
    }

    [Fact]
    public void StackAllocatedBuffer() {
        var vsw = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
        Assert.Equal("", vsw.ToString());
    }

    [Fact]
    public void Write_Single() {
        var vsw = new ValueSyntaxWriter();

        vsw.Write("Nanashi");

        Assert.Equal("Nanashi", vsw.ToString());
    }

    [Fact]
    public void Write_Multiple() {
        var vsw = new ValueSyntaxWriter();

        vsw.Write("Nanashi");
        vsw.Write(" ");
        vsw.Write("Mumei");

        Assert.Equal("Nanashi Mumei", vsw.ToString());
    }

    [Fact]
    public void WriteFormatted() {
        var vsw = new ValueSyntaxWriter();

        vsw.WriteFormatted($"Hello, {typeof(ValueSyntaxWriterTests):g}");

        Assert.Equal($"Hello, global::{typeof(ValueSyntaxWriterTests).FullName}", vsw.ToString());
    }

    [Fact]
    public void Write_WithIndent() {
        var vsw = new ValueSyntaxWriter();

        vsw.Write("Line 1");
        vsw.WriteLine();

        vsw.Indent();
        vsw.WriteLine("Indented Line");

        vsw.Indent();
        vsw.WriteLine("Double Indented Line");

        vsw.Dedent();
        vsw.WriteLine("Line 2");

        vsw.Dedent();
        vsw.Write("Line 3");

        var expected = """
                       Line 1
                           Indented Line
                               Double Indented Line
                           Line 2
                       Line 3
                       """;

        var actual = vsw.ToString();
        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Write_StackAllocatedBuffer() {
        var vsw = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);

        vsw.Write("Hello, Mumei!");

        Assert.Equal("Hello, Mumei!", vsw.ToString());
    }

    [Fact]
    public void Write_StackAllocatedBuffer_WriteExceedsInitialBuffer() {
        var vsw = new ValueSyntaxWriter(stackalloc char[8]);

        vsw.Write("Hello, Mumei!");

        Assert.Equal("Hello, Mumei!", vsw.ToString());
    }

    [Fact]
    public void WriteLiteral() {
        var vsw = new ValueSyntaxWriter();

        vsw.WriteLiteral(42);

        vsw.WriteLiteral(" is the answer");

        Assert.Equal("42 is the answer", vsw.ToString());
    }

    [Fact]
    public void CopyTo_Span() {
        var vsw = new ValueSyntaxWriter();
        vsw.Write("Mumei");

        Span<char> span = new char["Mumei".Length];
        vsw.CopyTo(span);

        Assert.Equal("Mumei", new string(span));
    }

    [Fact]
    public void CopyTo_SyntaxWriter() {
        var vsw = new ValueSyntaxWriter();
        vsw.Write("Mumei");

        var targetWriter = new SyntaxWriter();
        vsw.CopyTo(ref targetWriter);

        Assert.Equal("Mumei", targetWriter.ToString());
    }
}