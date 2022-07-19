using System.Reflection.Emit;
using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.SyntaxWriters;

public class ClassSyntaxBuilderTest {
  [Fact]
  public void ToString_ReturnsClassSkeletonWithDefaultVisibility_WhenNoVisibilityIsProvided() {
    var sut = new ClassSyntaxBuilder("Test");

    sut.ToString().Should().Be(
      Line("internal class Test {") +
      Line("}")
    );
  }

  [Fact]
  public void ToString_ReturnsClassSkeletonWithSpecifiedVisibility() {
    var sut = new ClassSyntaxBuilder("Test", TypeDeclarationVisibility.Public);

    sut.ToString().Should().Be(
      Line("public class Test {") +
      Line("}")
    );
  }

  [Fact]
  public void ToString_ReturnsClassImplementationDerivingFromInterface() {
    var sut = new ClassSyntaxBuilder("Test");

    sut.AddInterfaceImplementation<IDisposable>();

    sut.ToString().Should().Be(
      Line("internal class Test : System.IDisposable {") +
      Line("}")
    );
  }

  [Fact]
  public void ToString_ReturnsClassImplementationDerivingFromMultipleInterfaces() {
    var sut = new ClassSyntaxBuilder("Test");

    sut.AddInterfaceImplementation<IDisposable>();
    sut.AddInterfaceImplementation<ICloneable>();

    sut.ToString().Should().Be(
      Line("internal class Test : System.IDisposable, System.ICloneable {") +
      Line("}")
    );
  }

  [Fact]
  public void ToString_ReturnsClassImplementationDerivingFromBaseType() {
    var sut = new ClassSyntaxBuilder("Test");

    sut.DefineBaseType<Attribute>();

    sut.ToString().Should().Be(
      Line("internal class Test : System.Attribute {") +
      Line("}")
    );
  }

  [Fact]
  public void ToString_ReturnsClassImplementationDerivingFromBaseTypeAndInterface() {
    var sut = new ClassSyntaxBuilder("Test");

    sut.AddInterfaceImplementation<IDisposable>();
    sut.DefineBaseType<Attribute>();

    sut.ToString().Should().Be(
      Line("internal class Test : System.Attribute, System.IDisposable {") +
      Line("}")
    );
  }

  [Fact]
  public void AddInterfaceImplementation_Throws_WhenTypeIsNotAnInterface() {
    var sut = new ClassSyntaxBuilder("Test");

    var ex = Record.Exception(() => sut.AddInterfaceImplementation<Attribute>());

    ex.Should().BeOfType<ArgumentException>();
  }

  [Fact]
  public void DefineBaseType_Throws_WhenABaseTypeHasAlreadyBeenDefined() {
    var sut = new ClassSyntaxBuilder("Test");

    sut.DefineBaseType<Attribute>();
    var ex = Record.Exception(() => sut.DefineBaseType<Attribute>());

    ex.Should().BeOfType<Exception>();
  }

  [Fact]
  public void AddField_AddsFieldWithSpecifiedTypeAndName() {
    var sut = new ClassSyntaxBuilder("Test");

    sut.AddField<int?>();

    //sut.ToString().Should().Be(
    //  Line("internal class Test {") +
    //  Line("  private Nullable<int> field;") +
    //  Line("}")
    //);
  }

  [Fact]
  public void AddField_AddsFieldWithSpecifiedTypeAndNameAndDefaultValue() {
  }

  [Fact]
  public void AddField_AddsFieldWithSpecifiedNullableTypeAndNameAndDefaultValue() {
  }
}