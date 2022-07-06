using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxBuilders;

public class SyntaxConfiguration {
  public AttributeUsage[] Attributes = Array.Empty<AttributeUsage>();
  public string Name;
  public SyntaxVisibility Visibility = SyntaxVisibility.None;
}