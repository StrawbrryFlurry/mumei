using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.SyntaxNodes.Members;

public class FieldSyntaxTest {
  private readonly SyntaxTypeContext? _context = new();

  [Fact]
  public void WriteAsSyntax_WritesFieldText_WhenCalled() {
    var field = new FieldSyntax("field", null!) {
      Type = typeof(string)
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("String field;"));
  }
}