using System.Linq.Expressions;
using System.Reflection;

namespace Mumei.CodeGen.SyntaxNodes;

public class SyntaxExpressionVisitor : ExpressionVisitor {
  private const string CompilerGeneratedClassPrefix = "<>c__";

  protected override Expression VisitMemberInit(MemberInitExpression node) {
    return base.VisitMemberInit(node);
  }

  protected override Expression VisitBinary(BinaryExpression node) {
    return base.VisitBinary(node);
  }

  protected override Expression VisitBlock(BlockExpression node) {
    return base.VisitBlock(node);
  }

  protected override Expression VisitConditional(ConditionalExpression node) {
    return base.VisitConditional(node);
  }

  protected override Expression VisitConstant(ConstantExpression node) {
    return base.VisitConstant(node);
  }

  protected override Expression VisitDefault(DefaultExpression node) {
    return base.VisitDefault(node);
  }

  protected override Expression VisitDynamic(DynamicExpression node) {
    return base.VisitDynamic(node);
  }

  protected override Expression VisitExtension(Expression node) {
    return base.VisitExtension(node);
  }

  protected override Expression VisitGoto(GotoExpression node) {
    return base.VisitGoto(node);
  }

  protected override Expression VisitIndex(IndexExpression node) {
    return base.VisitIndex(node);
  }

  protected override Expression VisitInvocation(InvocationExpression node) {
    return base.VisitInvocation(node);
  }

  protected override Expression VisitLabel(LabelExpression node) {
    return base.VisitLabel(node);
  }

  protected override Expression VisitLambda<T>(Expression<T> node) {
    return base.VisitLambda(node);
  }

  protected override Expression VisitLoop(LoopExpression node) {
    return base.VisitLoop(node);
  }

  protected override Expression VisitMember(MemberExpression node) {
    if (IsClosureWrappedConstantExpression(node, out var wrappedTarget)) {
      return UnwrapClosureWrappedConstantExpression(wrappedTarget!, (FieldInfo)node.Member);
    }

    var expression = Visit(node.Expression);

    if (expression is not ConstantExpression target) {
      return base.VisitMember(node);
    }

    if (IsValueHolderSyntax(target.Value)) {
      return TransformValueHolderSyntax(target, node.Member);
    }

    return base.VisitMember(node);
  }

  private bool IsValueHolderSyntax(object value) {
    var interfaces = value.GetType().GetInterfaces();
    return interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueHolderSyntax<>));
  }

  private Expression TransformValueHolderSyntax(ConstantExpression target, MemberInfo member) {
    // We allow users to imitate variable access though the "Value"
    // property. To reflect that in the generated expression, we need to
    // replace the current target with the variable instance.
    if (member.Name != nameof(IValueHolderSyntax<object>.Value)) {
      return Expression.MakeMemberAccess(target, member);
    }

    var valueHolder = target.Value;
    var identifierProperty = valueHolder.GetType().GetProperty(nameof(IValueHolderSyntax<object>.Identifier))!;
    var valueProperty = valueHolder.GetType().GetProperty(nameof(IValueHolderSyntax<object>.Value))!;
    return Expression.Variable(valueProperty.PropertyType, (string)identifierProperty.GetValue(valueHolder));
  }

  internal bool IsClosureWrappedConstantExpression(MemberExpression expression, out ConstantExpression? target) {
    target = Visit(expression.Expression) as ConstantExpression;

    if (target is { NodeType: ExpressionType.Constant } expressionTarget) {
      return IsCompilerGeneratedClosureWrapper(expressionTarget);
    }

    return false;
  }

  private static bool IsCompilerGeneratedClosureWrapper(ConstantExpression? expression) {
    return expression?.Type?.Name?.StartsWith(CompilerGeneratedClassPrefix) ?? false;
  }

  /// <summary>
  ///   When referencing variables in a lambda such that
  ///   the value is captured in a closure, the compiler will
  ///   generate a helper class wrapping the value. Because we
  ///   cannot use that helper class, we need to get access to
  ///   wrapped value. To make the expression work despite this
  ///   wrapper class, the compiler adds a MemberExpression before
  ///   the actual value.
  ///   The following lambda:
  ///   <code>
  /// var v = "A";
  /// Expression{Func{bool}} e = () => v == "A";
  /// </code>
  ///   will generate an expression tree like this:
  ///   <code>
  ///   LambdaExpression
  ///   => BinaryExpression
  ///   => Left
  ///   => MemberExpression { Field: "V" }
  ///   => ConstantExpression { Value: "__c__DisplayClass" }
  ///   </code>
  ///   We can use the method info in the expression tree above
  ///   the constant expression holding the value to get the field
  ///   value that holds the closure wrapped value.
  /// </summary>
  /// <param name="expression">The constant expression containing the wrapper class</param>
  /// <param name="wrappedValueField">The field containing the wrapped value</param>
  /// <returns>A ConstantExpression containing the wrapped value</returns>
  internal ConstantExpression UnwrapClosureWrappedConstantExpression(
    ConstantExpression expression,
    FieldInfo wrappedValueField
  ) {
    var wrappedValue = wrappedValueField.GetValue(expression.Value);
    return Expression.Constant(wrappedValue);
  }


  protected override Expression VisitNew(NewExpression node) {
    return base.VisitNew(node);
  }

  protected override Expression VisitParameter(ParameterExpression node) {
    return base.VisitParameter(node);
  }

  protected override Expression VisitSwitch(SwitchExpression node) {
    return base.VisitSwitch(node);
  }

  protected override Expression VisitTry(TryExpression node) {
    return base.VisitTry(node);
  }

  protected override Expression VisitUnary(UnaryExpression node) {
    return base.VisitUnary(node);
  }

  protected override CatchBlock VisitCatchBlock(CatchBlock node) {
    return base.VisitCatchBlock(node);
  }

  protected override Expression VisitDebugInfo(DebugInfoExpression node) {
    return base.VisitDebugInfo(node);
  }

  protected override ElementInit VisitElementInit(ElementInit node) {
    return base.VisitElementInit(node);
  }

  protected override LabelTarget VisitLabelTarget(LabelTarget node) {
    return base.VisitLabelTarget(node);
  }

  protected override Expression VisitListInit(ListInitExpression node) {
    return base.VisitListInit(node);
  }

  protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) {
    return base.VisitMemberAssignment(node);
  }

  protected override MemberBinding VisitMemberBinding(MemberBinding node) {
    return base.VisitMemberBinding(node);
  }

  protected override Expression VisitMethodCall(MethodCallExpression node) {
    return base.VisitMethodCall(node);
  }

  protected override Expression VisitNewArray(NewArrayExpression node) {
    return base.VisitNewArray(node);
  }

  protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
    return base.VisitRuntimeVariables(node);
  }

  protected override SwitchCase VisitSwitchCase(SwitchCase node) {
    return base.VisitSwitchCase(node);
  }

  protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
    return base.VisitTypeBinary(node);
  }

  protected override MemberListBinding VisitMemberListBinding(MemberListBinding node) {
    return base.VisitMemberListBinding(node);
  }

  protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node) {
    return base.VisitMemberMemberBinding(node);
  }
}