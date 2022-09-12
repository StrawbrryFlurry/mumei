using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Members;

public class AccessorSyntaxTests {
  [Fact]
  public void WriteAsSyntax_WritesAutoGetter_WhenAccessorIsGetterAndHasNoBody() {
    var sut = new AccessorSyntax(AccessorType.Get);

    sut.WriteSyntaxAsString().Should().Be("get;");
  }

  [Fact]
  public void WriteAsSyntax_WritesAutoGetterWithVisibiltiy() {
    var sut = new AccessorSyntax(AccessorType.Get, null, SyntaxVisibility.Private);

    sut.WriteSyntaxAsString().Should().Be("private get;");
  }

  [Fact]
  public void WriteAsSyntax_WritesNoVisibility_WhenVisibilityIsPublicOrNone() {
    var sut = new AccessorSyntax(AccessorType.Get, null, SyntaxVisibility.Public);

    sut.WriteSyntaxAsString().Should().Be("get;");
  }

  [Fact]
  public void WriteAsSyntax_WritesGetterWithBody_WhenAccessorHasBody() {
    var block = new BlockSyntaxBuilder();
    block.Return(Expression.Constant("Foo"));

    var sut = new AccessorSyntax(AccessorType.Get, block.Build());

    sut.WriteSyntaxAsString().Should().Be(Line("get {") +
                                          IndentedLine("return \"Foo\";", 1) +
                                          "}");
  }

  [Fact]
  public void WriteAsSyntax_WritesCorrectIndentationLevel() {
    var sut = new AccessorSyntax(AccessorType.Get);

    var writer = new TypeAwareSyntaxWriter();
    writer.Indent();

    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(IndentationString("get;", 1));
  }

  [Fact]
  public void WriteAsSyntax_WritesCorrectIndentationLevel_WhenVisibiltiyIsDefined() {
    var sut = new AccessorSyntax(AccessorType.Get, null, SyntaxVisibility.Private);
    var writer = new TypeAwareSyntaxWriter();

    writer.Indent();
    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(IndentationString("private get;", 1));
  }

  [Fact]
  public void Ctor_Throws_WhenVisibilityIsInvalid() {
    var action = () => new AccessorSyntax(AccessorType.Get, null, SyntaxVisibility.Async);

    action.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Ctor_SetsSelfAsParentOnBlock() {
    var block = new BlockSyntaxBuilder().Build();
    var sut = new AccessorSyntax(AccessorType.Get, block);

    block.Parent.Should().Be(sut);
  }

  [Fact]
  public void Clone_ReturnsNewInstanceWithSameTypeAndCloneOfBlock() {
    var blockBuilder = new BlockSyntaxBuilder();
    blockBuilder.Return(Expression.Constant("Foo"));
    var block = blockBuilder.Build();
    var sut = new AccessorSyntax(AccessorType.Get, block);

    var clone = sut.Clone<AccessorSyntax>();

    clone.AccessorType.Should().Be(sut.AccessorType);
    clone.Body!.Statements.Should().HaveSameCount(sut.Body!.Statements);
    clone.Body.Parent.Should().Be(clone);
  }
}