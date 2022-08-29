using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public interface ICallableSyntax<out T> {
  T Invoke { get; }
}

public class MethodSyntax<TCallSignature> : Syntax, ICallableSyntax<TCallSignature> {
  public TCallSignature Invoke { get; }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }
}