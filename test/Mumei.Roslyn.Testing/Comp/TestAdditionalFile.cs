using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mumei.Roslyn.Testing.Comp;

public sealed class TestAdditionalFile : AdditionalText {
  private readonly SourceText _text;

  public TestAdditionalFile(string path, string text) {
    Path = path;
    _text = SourceText.From(text);
  }

  public override SourceText GetText(CancellationToken cancellationToken = new()) {
    return _text;
  }

  public override string Path { get; }
}