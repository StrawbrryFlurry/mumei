using FluentAssertions;
using Mumei.CodeGen.Syntax;
using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.Syntax.Members;

public class FieldSyntaxTest {
  private readonly SyntaxTypeContext _context = new();

  [Fact]
  public void WriteAsSyntax_WritesFieldText_WhenCalled() {
    var config = new MemberSyntaxConfiguration("field") {
      Type = typeof(string)
    };
    var field = new FieldSyntax(config);
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("String field;"));
  }
}