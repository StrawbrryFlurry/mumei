using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   A block of statements.
/// </summary>
public class BlockSyntax : StatementSyntax {
  public BlockSyntax(Syntax? parent = null) : base(parent) { }

  public IEnumerable<Syntax> Statements { get; set; }

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