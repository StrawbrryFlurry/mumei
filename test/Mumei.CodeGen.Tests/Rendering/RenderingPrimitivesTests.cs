namespace Mumei.CodeGen.Tests.Rendering;

public sealed class RenderingPrimitivesTests {
    [Fact]
    public void GetTypeFullName_NestedGenericTypes_RendersOuterToInnerWithOwnArguments() {
        var name = RuntimeTypeSerializer.GetTypeFullName(typeof(Outer<int>.Inner<string>));

        Assert.Equal("global::Mumei.CodeGen.Tests.Rendering.Outer<int>.Inner<string>", name);
    }

    [Fact]
    public void GetTypeFullName_ArraysAndNullables_RendersCSharpSyntax() {
        Assert.Equal("int?[,]", RuntimeTypeSerializer.GetTypeFullName(typeof(int?[,])));
        Assert.Equal(
            "global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.List<int>>",
            RuntimeTypeSerializer.GetTypeFullName(typeof(Dictionary<string, List<int>>))
        );
    }

    [Fact]
    public void Format_EscapesStringsAndCharacters() {
        Assert.Equal("\"a\\\"b\\\\c\\n\\u0001\"", CSharpLiteral.Format("a\"b\\c\n\u0001"));
        Assert.Equal("'\\''", CSharpLiteral.Format('\''));
    }

    [Fact]
    public void Format_UsesInvariantSuffixedNumbersAndQualifiedEnums() {
        Assert.Equal("1.5F", CSharpLiteral.Format(1.5f));
        Assert.Equal("10UL", CSharpLiteral.Format(10UL));
        Assert.Equal("global::System.DayOfWeek.Monday", CSharpLiteral.Format(DayOfWeek.Monday));
        Assert.Equal("((global::System.DayOfWeek)42)", CSharpLiteral.Format((DayOfWeek)42));
    }

    [Fact]
    public void Value_RendersLiteral() {
        var tree = new Mumei.CodeGen.Rendering.SourceFileRenderTreeBuilder();
        tree.Value("x\"y");

        Assert.Equal("\"x\\\"y\"", tree.GetSourceText());
    }
}

public sealed class Outer<T> {
    public sealed class Inner<U>;
}
