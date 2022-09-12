using Mumei.CodeGen.SyntaxWriters;
using static Mumei.CodeGen.SyntaxWriters.SyntaxVisibility;

namespace Mumei.CodeGen.SyntaxNodes;

public enum AccessorType {
  Get,
  Set,
  Init
}

public sealed class AccessorSyntax : TypeSyntax {
  public AccessorSyntax(
    AccessorType type,
    BlockSyntax? body = null,
    SyntaxVisibility visibility = None,
    Syntax? parent = null) : base(type.ToString(), parent) {
    AssertIsValidVisibility(visibility);
    Visibility = visibility;
    AccessorType = type;
    body?.SetParent(this);
    Body = body;
  }

  public static AccessorSyntax AutoGet => new(AccessorType.Get);
  public static AccessorSyntax AutoSet => new(AccessorType.Set);
  public static AccessorSyntax AutoInit => new(AccessorType.Init);

  public AccessorType AccessorType { get; }

  public BlockSyntax? Body { get; }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.WriteLineStart();

    if (Visibility is not Public /* Accessors are implicitly public */) {
      writer.Write(Visibility);
    }

    writer.Write(GetIdentifier());

    if (Body is null) {
      writer.Write(";");
      return;
    }

    writer.Write(" ");
    Body.WriteAsSyntax(writer);
  }

  public override Syntax Clone() {
    var body = Body?.Clone<BlockSyntax>();
    return new AccessorSyntax(AccessorType, body, Visibility);
  }

  public override string GetIdentifier() {
    return AccessorType.ToString().ToLower();
  }

  private void AssertIsValidVisibility(SyntaxVisibility visibility) {
    if (visibility is not None and not Public and not Internal and not Private and not Protected) {
      throw new ArgumentException($"Invalid accessor visibility {visibility}");
    }
  }
}