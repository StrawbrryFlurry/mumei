using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Members;

public class AccessorListSyntaxTests {
  [Fact]
  public void Ctor_SetsSelfAsParentOfAccessors() {
    var getter = AccessorSyntax.AutoGet;
    var setter = AccessorSyntax.AutoSet;

    var sut = new AccessorListSyntax(getter, setter);

    getter.Parent.Should().Be(sut);
    setter.Parent.Should().Be(sut);
  }

  [Fact]
  public void Ctor_SetsAccessors() {
    var getter = AccessorSyntax.AutoGet;
    var setter = AccessorSyntax.AutoSet;

    var sut = new AccessorListSyntax(getter, setter);

    sut.Getter.Should().Be(getter);
    sut.Setter.Should().Be(setter);
  }

  [Fact]
  public void DefineGetter_SetsSelfAsParentAndSetsAccessor() {
    var getter = new AccessorSyntax(AccessorType.Get);
    var sut = new AccessorListSyntax();

    sut.DefineGetter(getter);

    getter.Parent.Should().Be(sut);
    sut.Getter.Should().Be(getter);
  }

  [Fact]
  public void DefineGetter_Throws_WhenAccessorTypeIsInvalid() {
    var getter = new AccessorSyntax(AccessorType.Set);
    var sut = new AccessorListSyntax();

    var action = () => sut.DefineGetter(getter);

    action.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void DefineSetter_SetsSelfAsParentAndSetsAccessor() {
    var setter = AccessorSyntax.AutoSet;
    var sut = new AccessorListSyntax();

    sut.DefineSetter(setter);

    setter.Parent.Should().Be(sut);
    sut.Setter.Should().Be(setter);
  }

  [Fact]
  public void DefineSetter_Throws_WhenAccessorTypeIsInvalid() {
    var getter = new AccessorSyntax(AccessorType.Get);
    var sut = new AccessorListSyntax();

    var action = () => sut.DefineSetter(getter);

    action.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void WriteAsSyntax_Throws_WhenAccessorHasNoGetter() {
    var sut = new AccessorListSyntax();

    var action = () => sut.WriteSyntaxAsString();

    action.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void WriteAsSyntax_WritesOnlyGetter_WhenListHasOnlyGetterDefined() {
    var sut = new AccessorListSyntax(AccessorSyntax.AutoGet);

    sut.WriteSyntaxAsString().Should().Be(Line("{") +
                                          IndentedLine("get;", 1) +
                                          "}");
  }

  [Fact]
  public void WriteAsSyntax_WritesOnlyGetter_WhenListHasGe() {
    var sut = new AccessorListSyntax(AccessorSyntax.AutoGet, AccessorSyntax.AutoSet);

    sut.WriteSyntaxAsString().Should().Be(Line("{") +
                                          IndentedLine("get;", 1) +
                                          IndentedLine("set;", 1) +
                                          "}");
  }

  [Fact]
  public void Clone_Throws_WhenAccessorDoesNotHaveValidGetter() {
    var sut = new AccessorListSyntax();

    var action = () => sut.Clone();

    action.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Clone_CreatesNewAccessorListWithSameAccessors() {
    var sut = new AccessorListSyntax(AccessorSyntax.AutoGet, AccessorSyntax.AutoSet);

    var clone = sut.Clone<AccessorListSyntax>();

    clone.Should().NotBeSameAs(sut);
    clone.Getter.Should().NotBeSameAs(sut.Getter);
    clone.Setter.Should().NotBeSameAs(sut.Setter);
  }
}