namespace Mumei.CodeGen.SyntaxNodes;

public interface IValueHolderDeclarationSyntax {
  public ExpressionSyntax? Initializer { get; }
  public Type Type { get; }
}

public interface IMemberValueHolderDeclarationSyntax : IValueHolderDeclarationSyntax {
}