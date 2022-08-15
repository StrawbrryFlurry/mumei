using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   A block of statements.
/// </summary>
public class BlockSyntax : Syntax {
  public IEnumerable<Syntax> Statements { get; set; }

  public BlockSyntax(string identifier) : base(identifier) {
  }

  public BlockSyntax(string identifier, Syntax? parent) : base(identifier, parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.WriteLine("{");
    writer.Indent();

    foreach (var statement in Statements) {
      statement.WriteAsSyntax(writer);
    }

    writer.UnIndent();
    writer.WriteLine("}");
  }
}