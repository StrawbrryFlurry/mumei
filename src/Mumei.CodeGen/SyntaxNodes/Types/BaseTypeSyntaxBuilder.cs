namespace Mumei.CodeGen.SyntaxNodes;

public abstract class BaseTypeSyntaxBuilder {
  public virtual FieldSyntax<TField> DefineField<TField>() {
    return null!;
  }

  public virtual object DefineMethod(string name, Func<BlockSyntaxBuilder> methodBuilder) {
    return null!;
  }

  public virtual object DefineMethod<TReturnType>(string name, Func<BlockSyntaxBuilder> methodBuilder) {
    return null!;
  }

  public virtual object DefineMethod<TReturnType, TArg1>(string name,
    Func<BlockSyntaxBuilder, TArg1> methodBuilder) {
    return null!;
  }

  public virtual object DefineMethod<TArg1>(string name, Func<BlockSyntaxBuilder, TArg1> methodBuilder) {
    return null!;
  }

  public virtual object DefineMethod<TArg1, TArg2>(string name,
    Func<BlockSyntaxBuilder, TArg1, TArg2> methodBuilder) {
    return null!;
  }

  public virtual object DefineMethod<TArg1, TArg2, TArg3>(string name,
    Func<BlockSyntaxBuilder, TArg1, TArg2, TArg3> methodBuilder) {
    return null!;
  }
}