using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.SyntaxWriters;

public class MemberSyntaxWriterTest {
  [Fact]
  public void WriteBlock_CreatesABlockWithTheSpecifiedStatements() {
    var builder = new MemberSyntaxWriter(0, null);

    builder.WriteBlock(() => {
      builder.WriteLine("line 1");
      builder.WriteLine("line 2");
    });

    Line();
  }
}