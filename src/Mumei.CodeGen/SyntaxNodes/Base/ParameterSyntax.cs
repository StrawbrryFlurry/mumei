using System.Linq.Expressions;

namespace Mumei.CodeGen.SyntaxNodes;

public class Param {
  private Param(Type type, string name) {
    Name = name;
    Type = type;
  }

  public Type Type { get; }
  public string Name { get; }

  public static Param Create<TParameter>(string name) {
    return new Param(typeof(TParameter), name);
  }

  public static Param Create(Type type, string name) {
    return new Param(type, name);
  }
}

public class ParameterSyntax : ParameterSyntax<object> {
  public ParameterSyntax(Expression expressionNode, Syntax? parent = null) : base(expressionNode, parent) { }

  public static object MakeGenericParameter(Type type, string name) {
    var genericParameterSyntax = typeof(ParameterSyntax<>).MakeGenericType(type);
    return Activator.CreateInstance(genericParameterSyntax, Expression.Constant(name))!;
  }
}

public class ParameterSyntax<TParameterType> : ExpressionSyntax, IValueHolderSyntax<TParameterType> {
  public ParameterSyntax(Expression expressionNode, Syntax? parent = null) : base(expressionNode, parent) {
    Identifier = expressionNode.ToString();
  }

  public string Identifier { get; }

  public TParameterType Value { get; set; } = default!;
}