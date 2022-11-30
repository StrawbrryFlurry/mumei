using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.CodeGen.Tests.SyntaxNodes.Members;

public class MethodHelpersTests {
  [Fact]
  public void GetFunctionDefinition_Throws_WhenTypeIsNeitherActionOrFunc() {
    var action = () => MethodHelpers.GetFunctionDefinition(typeof(string), out var arguments);

    action.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void GetFunctionDefinition_ReturnsVoidWithNoArguments_WhenTypeIsAction() {
    var returnType = MethodHelpers.GetFunctionDefinition(typeof(Action), out var arguments);

    returnType.Should().Be(typeof(void));
    arguments.Should().BeEmpty();
  }

  [Fact]
  public void GetFunctionDefinition_ReturnsVoidWithOneArgument_WhenTypeIsActionT1() {
    var returnType = MethodHelpers.GetFunctionDefinition(typeof(Action<string>), out var arguments);

    returnType.Should().Be(typeof(void));
    arguments.Should().HaveCount(1);
    arguments[0].Should().Be(typeof(string));
  }

  [Fact]
  public void GetFunctionDefinition_ReturnsVoidWithOneArgument_WhenTypeIsActionT16() {
    var actionType =
      typeof(
        Action<string, string, string, string, string, string, string, string, string, string, string, string,
          string, string, string, int>
      );
    var returnType = MethodHelpers.GetFunctionDefinition(actionType, out var arguments);

    returnType.Should().Be(typeof(void));
    arguments.Should().HaveCount(16);
    arguments[15].Should().Be(typeof(int));
  }

  [Fact]
  public void GetFunctionDefinition_ReturnsReturnTypeWithNoArguments_WhenTypeIsFuncT1() {
    var returnType = MethodHelpers.GetFunctionDefinition(typeof(Func<string>), out var arguments);

    returnType.Should().Be(typeof(string));
    arguments.Should().HaveCount(0);
  }

  [Fact]
  public void GetFunctionDefinition_ReturnsReturnTypeWith15Arguments_WhenTypeIsFuncT16() {
    var funcType =
      typeof(
        Func<string, string, string, string, string, string, string, string, string, string, string, string,
          string, string, string, int>
      );
    var returnType = MethodHelpers.GetFunctionDefinition(funcType, out var arguments);

    returnType.Should().Be(typeof(int));
    arguments.Should().HaveCount(15);
    arguments[14].Should().Be(typeof(string));
  }
}