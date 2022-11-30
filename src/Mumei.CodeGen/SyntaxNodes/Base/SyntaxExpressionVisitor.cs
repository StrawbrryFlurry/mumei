using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Mumei.CodeGen.Expressions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class SyntaxExpressionVisitor : ExpressionVisitor {
  private const string CompilerGeneratedClassPrefix = "<>c__";
  private readonly SyntaxTypeContext _typeContext;

  public SyntaxExpressionVisitor(SyntaxTypeContext typeContext) {
    _typeContext = typeContext;
  }

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
    if (node.Value is bool b) {
      return new BooleanExpression(b);
    }

    if (node.Value is not Type type) {
      return base.VisitConstant(node);
    }

    _typeContext.IncludeTypeNamespace(type);
    // Types in constant expressions are not stringified as typeof expressions.
    return new TypeExpression(type);
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
    var expression = Visit(node.Expression)!;

    if (expression is not MemberExpression member) {
      return base.VisitInvocation(node);
    }

    if (member.Expression is not ConstantExpression target) {
      return base.VisitInvocation(node);
    }

    return TransformInvocationInvokableExpression(node, target, member.Member, node.Arguments);
  }

  private bool IsInvokableInvocation(object target) {
    var interfaces = target.GetType().GetInterfaces();

    return interfaces
      .Select(@interface => @interface.GetGenericTypeDefinition())
      .Any(genericInterface => genericInterface == typeof(IInvokable<>));
  }

  private Expression TransformInvocationInvokableExpression(
    InvocationExpression node,
    ConstantExpression target,
    MemberInfo member,
    ReadOnlyCollection<Expression> arguments
  ) {
    if (!IsInvokableInvocation(target.Value)) {
      return base.VisitInvocation(node);
    }

    if (member.Name != nameof(IInvokable<object>.Invoke)) {
      return base.VisitInvocation(node);
    }

    var invokeProperty = (PropertyInfo)member;
    var returnType = GetInvokableReturnType(invokeProperty.PropertyType);

    return MakeInvokeInvokableExpression(target, returnType, node.Arguments);
  }

  private Type GetInvokableReturnType(Type invokableMethod) {
    if (invokableMethod == typeof(Delegate)) {
      return typeof(object);
    }

    try {
      return MethodHelpers.GetFunctionDefinition(invokableMethod, out _);
    }
    catch {
      throw new InvalidOperationException("`Invoke` property of the invokable syntax is not a valid function.");
    }
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
    var valueHolderType = valueHolder.GetType();
    var identifier = GetSyntaxIdentifier(valueHolderType, valueHolder);
    var valueProperty = valueHolderType.GetProperty(nameof(IValueHolderSyntax<object>.Value))!;
    return Expression.Variable(valueProperty.PropertyType, identifier);
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
    var expression = node.Object;

    if (expression is null) {
      return base.VisitMethodCall(node);
    }

    expression = Visit(expression);

    if (expression is not ConstantExpression target) {
      return base.VisitMethodCall(node);
    }

    return TransformMethodCallToInvokableExpression(node, target);
  }

  private Expression TransformMethodCallToInvokableExpression(
    MethodCallExpression node,
    ConstantExpression target
  ) {
    if (!IsDynamicInvocation(target.Value)) {
      return base.VisitMethodCall(node);
    }

    if (node.Method.Name != nameof(IDynamicallyInvokable.Invoke)) {
      return base.VisitMethodCall(node);
    }

    // The invoke method always has a single argument which is the params array.
    var arguments = GetMethodCallArgumentsFromImplicitParamsParameter(node.Arguments.First());

    return MakeInvokeInvokableExpression(target, typeof(object), arguments);
  }

  private Expression[] GetMethodCallArgumentsFromImplicitParamsParameter(Expression expression) {
    if (expression is not NewArrayExpression newArrayExpression) {
      return Array.Empty<Expression>();
    }

    // All members of the array init expression are cast to object because the params
    // parameter takes an object[]. We don't need the type conversion.
    return newArrayExpression.Expressions.Select(StripConvertExpression).ToArray();
  }

  private Expression StripConvertExpression(Expression expression) {
    if (expression is not UnaryExpression convertExpression) {
      return expression;
    }

    return convertExpression.Operand;
  }

  private bool IsDynamicInvocation(object target) {
    return target is IDynamicallyInvokable;
  }

  private Expression MakeInvokeInvokableExpression(
    ConstantExpression target,
    Type returnType,
    IReadOnlyCollection<Expression> arguments) {
    var invokable = target.Value;
    var invokableType = invokable.GetType();
    var identifier = GetSyntaxIdentifier(invokableType, invokable);

    var updatedArguments = arguments.Select(Visit).ToArray();

    return new InvokeInvokableExpression(
      identifier,
      updatedArguments,
      returnType
    );
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

  private string GetSyntaxIdentifier(Type type, object instance) {
    var identifierProperty = type.GetProperty(nameof(ISyntaxIdentifier.Identifier))!;
    return (string)identifierProperty.GetValue(instance);
  }
}