using System.Linq.Expressions;
using System.Reflection;
using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.SyntaxNodes;

public delegate void BlockBuilder(BlockSyntaxBuilder builder);

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
  public VariableExpressionSyntax<TVariable> VariableDeclaration<TVariable>(string name,
    ExpressionSyntax? initializer = null) {
    var declaration = new VariableDeclarationStatementSyntax(typeof(TVariable), name, initializer);
    Statements.Add(declaration);

    return new VariableExpressionSyntax<TVariable>(name, declaration);
  }

  /// <inheritdoc cref="VariableDeclaration{TVariable}" />
  public object VariableDeclaration(Type variableType, string name,
    ExpressionSyntax? initializer = null) {
    var genericVariableDeclarationMethod = GenericVariableDeclarationMethodInfo.MakeGenericMethod(variableType);
    var args = new object?[] { name, initializer };
    return genericVariableDeclarationMethod.Invoke(this, args);
  }

  public void Statement(Expression<Action> statement) {
    Statements.Add(new ExpressionStatementSyntax(statement));
  }

  public void Statement<TReturn>(Expression<Func<TReturn>> statement) {
    Statements.Add(new ExpressionStatementSyntax(statement));
  }

  public void Assign<TVariable>(VariableExpressionSyntax<TVariable> variable, TVariable value) { }

  public IfStatementSyntax If(Expression<Func<bool>> condition, BlockBuilder body) {
    return If((ExpressionSyntax)condition, body);
  }

  public IfStatementSyntax If(ExpressionSyntax condition, BlockBuilder body) {
    var block = MakeBlock(body);
    var statement = new IfStatementSyntax(condition, block);

    Statements.Add(statement);

    return statement;
  }

  private BlockSyntax MakeBlock(BlockBuilder blockFactory) {
    var blockBuilder = new BlockSyntaxBuilder();
    blockFactory.Invoke(blockBuilder);
    return blockBuilder.Build();
  }

  public BlockSyntax Build() {
    var block = new BlockSyntax(_parent);

    foreach (var statement in Statements) {
      block.AddStatement(statement);
    }

    return block;
  }
}