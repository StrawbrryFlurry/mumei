using System.Linq.Expressions;
using System.Reflection;
using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.SyntaxNodes;

public delegate void BlockBuilder(BlockSyntaxBuilder builder);

public static class BlockBuilderExtensions {
  public static BlockSyntax Build(this BlockBuilder builder) {
    var blockBuilder = new BlockSyntaxBuilder();
    builder(blockBuilder);
    return blockBuilder.Build();
  }
}

public class BlockSyntaxBuilder {
  private static readonly MethodInfo GenericVariableDeclarationMethodInfo =
    typeof(BlockSyntaxBuilder).SelectGenericMethodOverload(nameof(VariableDeclaration));

  private readonly Syntax? _parent;

  internal readonly List<StatementSyntax> Statements = new();

  public BlockSyntaxBuilder(Syntax? parent = null) {
    _parent = parent;
  }

  /// <summary>
  ///   Creates a new local variable within the current block.
  ///   The variable returned has the type of the actual variable type
  ///   and it's properties can be accessed like normal within expressions.
  ///   Beware that changes made to the variable are not reflected outside of expressions
  ///   that support the usage of local variables. Similarly, any value assigned or changed
  ///   outside of an expression body is ignored and might lead to runtime errors if the default
  ///   value for a given type might be null.
  /// </summary>
  /// <param name="name">The name of the variable</param>
  /// <param name="initializer"></param>
  /// <typeparam name="TVariable"></typeparam>
  /// <returns>A placeholder variable instance that can be used as a reference in expressions within this block</returns>
  public VariableExpressionSyntax<TVariable> VariableDeclaration<TVariable>(
    string name,
    ExpressionSyntax? initializer = null
  ) {
    var declaration = new VariableDeclarationStatementSyntax(typeof(TVariable), name, initializer);
    Statements.Add(declaration);

    return new VariableExpressionSyntax<TVariable>(name, declaration);
  }

  /// <inheritdoc cref="VariableDeclaration{TVariable}" />
  public VariableExpressionSyntax VariableDeclaration(
    Type variableType,
    string name,
    ExpressionSyntax? initializer = null
  ) {
    var declaration = new VariableDeclarationStatementSyntax(variableType, name, initializer);
    Statements.Add(declaration);

    return new VariableExpressionSyntax(variableType, name, declaration);
  }

  public void Statement(StatementSyntax statement) {
    Statements.Add(statement);
  }

  public void Statement(Expression expression) {
    Statements.Add(new ExpressionStatementSyntax(expression));
  }

  public void Statement(Expression<Action> statement) {
    Statements.Add(new ExpressionStatementSyntax(statement));
  }

  public void Statement<TReturn>(Expression<Func<TReturn>> statement) {
    Statements.Add(new ExpressionStatementSyntax(statement));
  }

  public void Assign<TValue>(IValueHolderSyntax<TValue> target, Expression<Func<TValue>> valueExpression) {
    Expression assignmentExpression;
    if (target is ExpressionSyntax expressionTarget) {
      assignmentExpression = Expression.Assign(expressionTarget, valueExpression.Body);
    }
    else {
      var targetVariable = Expression.Variable(typeof(TValue), target.Identifier);
      assignmentExpression = Expression.Assign(targetVariable, valueExpression.Body);
    }

    Statements.Add(new ExpressionStatementSyntax(assignmentExpression));
  }

  public void Assign<TValue>(IValueHolderSyntax<TValue> target, ExpressionSyntax refValue) {
    Expression assignmentExpression;
    if (target is ExpressionSyntax expressionTarget) {
      assignmentExpression = Expression.Assign(expressionTarget, refValue);
    }
    else {
      var targetVariable = Expression.Variable(typeof(TValue), target.Identifier);
      assignmentExpression = Expression.Assign(targetVariable, refValue);
    }

    Statements.Add(new ExpressionStatementSyntax(assignmentExpression));
  }

  public void Assign<TSyntax, TValue>(TSyntax target, TValue value)
    where TSyntax : ExpressionSyntax, IValueHolderSyntax<TValue> {
    var assignment = Expression.Assign(target, Expression.Constant(value));
    Statements.Add(new ExpressionStatementSyntax(assignment));
  }

  public IfStatementSyntax If(Expression<Func<bool>> condition, BlockBuilder body) {
    return If((ExpressionSyntax)condition, body);
  }

  public void Return() {
    Statements.Add(new ReturnStatementSyntax());
  }

  public void Return(ExpressionSyntax value) {
    Statements.Add(new ReturnStatementSyntax(value));
  }

  public IfStatementSyntax If(ExpressionSyntax condition, BlockBuilder body) {
    var block = body.Build();
    var statement = new IfStatementSyntax(condition, block);

    Statements.Add(statement);

    return statement;
  }

  public BlockSyntax Build() {
    var block = new BlockSyntax(_parent);

    foreach (var statement in Statements) {
      block.AddStatement(statement);
    }

    return block;
  }
}