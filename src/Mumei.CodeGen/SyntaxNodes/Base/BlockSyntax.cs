using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   A block of statements.
/// </summary>
public class BlockSyntax : StatementSyntax {
  protected readonly List<StatementSyntax> _statements = new();

  public BlockSyntax(Syntax? parent = null) : base(parent) { }
  public IEnumerable<StatementSyntax> Statements => _statements;

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.WriteLine("{");
    writer.Indent();

    foreach (var statement in Statements) {
      writer.WriteLineStart();
      statement.WriteAsSyntax(writer);
      writer.WriteLine();
    }

    writer.UnIndent();
    writer.Write("}");
  }

  internal void AddStatement(StatementSyntax statement) {
    statement.SetParent(this);
    _statements.Add(statement);
  }
}