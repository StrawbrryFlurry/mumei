using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public interface IDynamicallyInvokable : ISyntaxIdentifier {
  public object? Invoke(params object[] parameters);
}

public interface IInvokable<out T> : ISyntaxIdentifier {
  public T Invoke { get; }
}

public class MethodSyntax : MemberSyntax, IDynamicallyInvokable {
  public MethodSyntax(Type type, string identifier, Syntax? parent = null) : base(type, identifier, parent) { }

  protected internal override int Priority => 10;

  public object? Invoke(params object[] parameters) {
    throw new NotSupportedException("This method is not supposed to be called from outside of Expression Bodies.");
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }

  public override Syntax Clone() {
    return new MethodSyntax(Type, Identifier);
  }
}

public class MethodSyntax<TCallSignature> : MemberSyntax, IInvokable<TCallSignature> {
  private readonly MethodSyntaxCore _method;

  public MethodSyntax(string identifier, Syntax? parent = null) : this(identifier, null, parent) { }

  public MethodSyntax(string identifier, BlockSyntax? body, Syntax? parent = null)
    : base(GetMethodReturnType(), identifier, parent) {
    // _method = new MethodSyntaxCore(Type, identifier,);
    // _method.Body = body;
//
    // SyntaxFactory.MethodDeclaration()

    _method = null!;
  }

  protected internal override int Priority => 10;

  private BlockSyntax? Body => _method.Body;

  public TCallSignature Invoke { get; } = default!;

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }

  public override Syntax Clone() {
    var body = Body?.Clone<BlockSyntax>();
    return new MethodSyntax<TCallSignature>(Identifier, body);
  }

  private static Type GetMethodReturnType() {
    try {
      return MethodHelpers.GetFunctionDefinition(typeof(TCallSignature), out _);
    }
    catch {
      throw new ArgumentException($"Type argument {typeof(TCallSignature)} is not a valid function definition.");
    }
  }
}

internal class MethodSyntaxCore : MemberSyntax {
  public BlockSyntax? Body;
  public Type[] ParameterTypes;
  public Type ReturnType;
  public Type[] TypeParameters;

  public MethodSyntaxCore(
    Type returnType,
    string identifier,
    Type[] parameterTypes,
    Type[] typeParameters,
    Syntax parent
  ) : base(returnType, identifier, parent) {
    ReturnType = returnType;
    ParameterTypes = parameterTypes;
    TypeParameters = typeParameters;

    Body = null!;
  }

  protected internal override int Priority => 10;

  public override Syntax Clone() {
    throw new NotImplementedException();
  }
}