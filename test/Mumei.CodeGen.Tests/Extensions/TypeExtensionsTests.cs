using System.Reflection;
using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.Tests.Extensions;

public class TypeExtensionsTests {
  [Fact]
  public void SelectGenericMethodOverload_ReturnsGenericMethodOverload_WhenTypeHasSingleGenericMethodWithName() {
    var action = () =>
      typeof(SingleGenericAndNonGenericMethod)
        .SelectGenericMethodOverload(nameof(SingleGenericAndNonGenericMethod.Test)
        );

    action().Should().BeAssignableTo<MethodInfo>();
  }

  [Fact]
  public void
    SelectGenericMethodOverload_ThrowsInvalidOperationException_WhenTypeHasMultipleGenericMethodsOfTheSameName() {
    var action = () =>
      typeof(MultipleGenericAndNonGenericMethod)
        .SelectGenericMethodOverload(nameof(MultipleGenericAndNonGenericMethod.Test)
        );

    action.Should().Throw<InvalidOperationException>();
  }


  [Fact]
  public void GetDefaultValue_ReturnsDefaultValueForSpecifiedType() {
    typeof(int).GetDefaultValue().Should().Be(0);
    typeof(char).GetDefaultValue().Should().Be('\0');
    typeof(bool).GetDefaultValue().Should().Be(false);
    typeof(int?).GetDefaultValue().Should().Be(null);
    typeof(string).GetDefaultValue().Should().Be(null);
    typeof(TestValueType).GetDefaultValue().Should().BeOfType<TestValueType>();
    typeof(TestReferenceType).GetDefaultValue().Should().BeNull();
  }

  private class SingleGenericAndNonGenericMethod {
    public void Test<TTest>() { }
    public void Test() { }
  }

  private class MultipleGenericAndNonGenericMethod {
    public void Test<TTest>() { }
    public void Test<TTest, TFooBar>() { }
    public void Test() { }
  }

  private struct TestValueType {
    public string Foo { get; set; }
  }

  private class TestReferenceType { }
}