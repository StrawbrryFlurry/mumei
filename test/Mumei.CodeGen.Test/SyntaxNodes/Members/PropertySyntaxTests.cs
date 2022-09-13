using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Members;

public class PropertySyntaxTests {
  [Fact]
  public void Ctor_SetsParentOnAccessorList() {
    var sut = MakeProperty();

    sut.Accessors.Parent.Should().Be(sut);
  }

  [Fact]
  public void DefineAutoGetter_DefinesAutoGetterAsGetAccessor() {
    var sut = MakeProperty();

    sut.DefineAutoGetter();
    var getter = sut.Accessors.Getter;

    getter!.Body.Should().BeNull();
    getter.Visibility.Should().BeOneOf(SyntaxVisibility.Public, SyntaxVisibility.None);
    getter.AccessorType.Should().Be(AccessorType.Get);
  }

  [Fact]
  public void DefineAutoInitSetter_DefinesAutoInitSetterGetterAsSetAccessor() {
    var sut = MakeProperty();

    sut.DefineAutoInitSetter();
    var getter = sut.Accessors.Setter;

    getter!.Body.Should().BeNull();
    getter.Visibility.Should().BeOneOf(SyntaxVisibility.Public, SyntaxVisibility.None);
    getter.AccessorType.Should().Be(AccessorType.Init);
  }

  [Fact]
  public void DefineAutoSetter_DefinesAutoSetterGetterAsSetAccessor() {
    var sut = MakeProperty();

    sut.DefineAutoSetter();
    var getter = sut.Accessors.Setter;

    getter!.Body.Should().BeNull();
    getter.Visibility.Should().BeOneOf(SyntaxVisibility.Public, SyntaxVisibility.None);
    getter.AccessorType.Should().Be(AccessorType.Set);
  }

  [Fact]
  public void DefineSetter_DefinesAutoSetterGetterAsSetAccessor() {
    var sut = MakeProperty();

    sut.DefineAutoSetter();
    var getter = sut.Accessors.Setter;

    getter!.Body.Should().BeNull();
    getter.Visibility.Should().BeOneOf(SyntaxVisibility.Public, SyntaxVisibility.None);
    getter.AccessorType.Should().Be(AccessorType.Set);
  }

  [Fact]
  public void DefineGetter_SetsGetterAsGetAccessor() {
    var sut = MakeProperty();

    var getter = new AccessorSyntax(AccessorType.Get, new BlockSyntax());
    sut.DefineGetter(getter);

    sut.Accessors.Getter.Should().Be(getter);
  }

  [Fact]
  public void DefineGetter_SetsSetterAsSetAccessor() {
    var sut = MakeProperty();

    var setter = new AccessorSyntax(AccessorType.Set, new BlockSyntax());
    sut.DefineSetter(setter);

    sut.Accessors.Setter.Should().Be(setter);
  }

  [Fact]
  public void Clone_CreatesNewPropertySyntaxWithSameAccessorsTypeAndIdentifier() {
    var sut = MakeProperty();

    sut.DefineAutoGetter();
    sut.DefineAutoSetter();

    var clone = sut.Clone<PropertySyntax>();

    clone.Should().BeOfType<PropertySyntax>();
    clone.Should().NotBeSameAs(sut);
    clone.Accessors.Should().NotBeSameAs(sut.Accessors);
    clone.Accessors.Getter.Should().NotBeSameAs(sut.Accessors.Getter);
    clone.Accessors.Setter.Should().NotBeSameAs(sut.Accessors.Setter);
    clone.Accessors.Getter!.AccessorType.Should().Be(sut.Accessors.Getter!.AccessorType);
    clone.Accessors.Setter!.AccessorType.Should().Be(sut.Accessors.Setter!.AccessorType);
    clone.Identifier.Should().Be(sut.Identifier);
  }

  [Fact]
  public void Clone_CreatesCloneOfGenericPropertySyntax() {
    var sut = new PropertySyntax<string>("property");

    sut.DefineAutoGetter();
    sut.DefineAutoSetter();

    var clone = sut.Clone<PropertySyntax<string>>();

    clone.Should().BeOfType<PropertySyntax<string>>();
    clone.Should().NotBeSameAs(sut);
    clone.Accessors.Should().NotBeSameAs(sut.Accessors);
    clone.Accessors.Getter.Should().NotBeSameAs(sut.Accessors.Getter);
    clone.Accessors.Setter.Should().NotBeSameAs(sut.Accessors.Setter);
    clone.Accessors.Getter!.AccessorType.Should().Be(sut.Accessors.Getter!.AccessorType);
    clone.Accessors.Setter!.AccessorType.Should().Be(sut.Accessors.Setter!.AccessorType);
    clone.Identifier.Should().Be(sut.Identifier);
  }

  [Fact]
  public void WriteAsSyntax_WritesTypeNameAndIdentifier() {
    var sut = new PropertySyntax<string>("Property");

    sut.DefineAutoGetter();

    sut.WriteSyntaxAsString().Should().Be(Line("String Property {") +
                                          IndentedLine("get;", 1) +
                                          "}");
  }

  [Fact]
  public void WriteAsSyntax_WritesVisibility_WhenVisibilityIsDefined() {
    var sut = new PropertySyntax<string>("Property");

    sut.DefineAutoGetter();
    sut.SetVisibility(SyntaxVisibility.Private);

    sut.WriteSyntaxAsString().Should().Be(Line("private String Property {") +
                                          IndentedLine("get;", 1) +
                                          "}");
  }

  [Fact]
  public void WriteAsSyntax_AddsPropertyTypeToTypeContext() {
    var sut = new PropertySyntax<SyntaxVisibility>("Property");

    sut.DefineAutoGetter();
    sut.SetVisibility(SyntaxVisibility.Private);

    sut.WriteSyntaxAsString(out var ctx);

    ctx.UsedNamespaces.Should().Contain(typeof(SyntaxVisibility).Namespace);
  }

  private PropertySyntax MakeProperty() {
    return new PropertySyntax(typeof(string), "property");
  }
}