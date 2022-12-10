using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Tests.SyntaxNodes.Base;

public class ExpressionSyntaxTests {
  [Fact]
  public void ToString_ReturnsExpressionAsString() {
    var sut = new ExpressionSyntax(Expression.UnaryPlus(Expression.Constant(1)));

    sut.ToString().Should().Be("+1");
  }

  [Fact]
  public void Clone_ReturnsNewExpressionSyntaxWithSameExpression() {
    var sut = new ExpressionSyntax(() => 10 + 20);

    var clone = sut.Clone();

    clone.ToString().Should().Be(sut.ToString());
  }

  [Fact]
  public void
    ParseExpressionToSyntaxString_ReplacesTargetCallWithSpecifiedValue_WhenTargetImplementsIValueHolderSyntax() {
    var closureVariable = new ImplementsIValueHolder<string> { Identifier = "Bar" };
    var sut = new ExpressionSyntax(() => closureVariable.Value == "Bar");

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Bar == \"Bar\"");
  }

  [Fact]
  public void ParseExpressionToSyntaxString_KeepsMemberExpressionAsIs_WhenTargetDoesNotImplementIValueHolderSyntax() {
    var closureVariable = Expression.Variable(typeof(string), "Bar");
    var sut = new ExpressionSyntax(() => closureVariable.Name == "Bar");

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Bar.Name == \"Bar\"");
  }

  [Fact]
  public void ParseExpressionToSyntaxString_KeepsMemberExpressionAsIs_WhenTargetIsNoExpression() {
    var closureVariable = "Foo";
    var sut = new ExpressionSyntax(() => closureVariable.Length == 5);

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("\"Foo\".Length == 5");
  }

  [Fact]
  public void
    ParseExpressionToSyntaxString_ReplacesCallableMemberWithCallMethod_WhenTargetImplementsIDynamicInvokable() {
    var callable = new ImplementsIInvokableDynamic { Identifier = "Foo" };
    var sut = new ExpressionSyntax(() => callable.Invoke());

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Foo()");
  }

  [Fact]
  public void
    ParseExpressionToSyntaxString_ReplacesCallableMemberWithCallMethod_WhenTargetImplementsIDynamicInvokableWithArguments() {
    var callable = new ImplementsIInvokableDynamic { Identifier = "Foo" };
    var sut = new ExpressionSyntax(() => callable.Invoke(true, 1));

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Foo(true, 1)");
  }

  [Fact]
  public void
    ParseExpressionToSyntaxString_ReplacesVariableReferences_WhenPassedAsArguments() {
    var argument = new ImplementsIValueHolder<bool> { Identifier = "Bar" };
    var callable = new ImplementsIInvokableFuncT3 { Identifier = "Foo" };
    var sut = new ExpressionSyntax(() => callable.Invoke(argument.Value, 1) == "foo");

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Foo(Bar, 1) == \"foo\"");
  }

  [Fact]
  public void
    ParseExpressionToSyntaxString_ReplacesCallableMemberWithCallMethod_WhenTargetImplementsIInvokableFuncT1() {
    var callable = new ImplementsIInvokableFuncT1 { Identifier = "Foo" };
    var sut = new ExpressionSyntax(() => callable.Invoke() == "foo");

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Foo() == \"foo\"");
  }

  [Fact]
  public void
    ParseExpressionToSyntaxString_ReplacesCallableMemberWithCallMethod_WhenTargetImplementsIInvokableFuncT3() {
    var callable = new ImplementsIInvokableFuncT3 { Identifier = "Foo" };
    var sut = new ExpressionSyntax(() => callable.Invoke(false, 1) == "foo");

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Foo(false, 1) == \"foo\"");
  }

  [Fact]
  public void
    ParseExpressionToSyntaxString_ReplacesCallableMemberWithCallMethod_WhenTargetImplementsIInvokableAction() {
    var callable = new ImplementsIInvokableAction { Identifier = "Foo" };
    var sut = new ExpressionSyntax(() => callable.Invoke());

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Foo()");
  }

  [Fact]
  public void
    ParseExpressionToSyntaxString_ReplacesCallableMemberWithCallMethod_WhenTargetImplementsIInvokableActionT2() {
    var callable = new ImplementsIInvokableActionT2 { Identifier = "Foo" };
    var sut = new ExpressionSyntax(() => callable.Invoke("foo", 1));

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Foo(\"foo\", 1)");
  }

  private class ImplementsIInvokableDynamic : IDynamicallyInvokable {
    public string Identifier { get; init; } = string.Empty;

    public object? Invoke(params object[] parameters) {
      throw new NotImplementedException();
    }
  }

  private class ImplementsIInvokableFuncT1 : IInvokable<Func<string>> {
    public string Identifier { get; init; } = default!;
    public Func<string> Invoke { get; } = default!;
  }

  private class ImplementsIInvokableFuncT3 : IInvokable<Func<bool, int, string>> {
    public string Identifier { get; init; } = default!;
    public Func<bool, int, string> Invoke { get; } = default!;
  }

  private class ImplementsIInvokableAction : IInvokable<Action> {
    public string Identifier { get; init; } = default!;
    public Action Invoke { get; } = default!;
  }

  private class ImplementsIInvokableActionT2 : IInvokable<Action<string, int>> {
    public string Identifier { get; init; } = default!;
    public Action<string, int> Invoke { get; } = default!;
  }

  private class ImplementsIValueHolder<T> : IValueHolderSyntax<T> {
    public string Identifier { get; set; } = default!;
    public T Value { get; set; } = default!;
  }
}