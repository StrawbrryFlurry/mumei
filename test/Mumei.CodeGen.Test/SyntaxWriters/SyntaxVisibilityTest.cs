using FluentAssertions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxWriters;

public class SyntaxVisibilityTest {
  [Fact]
  public void Tostring_ReturnsSingleVisibilityAsString() {
    var sut = SyntaxVisibility.Public;

    var actual = sut.ToVisibilityString();

    actual.Should().Be("public");
  }

  [Fact]
  public void ToString_ReturnsAllVisibilitiesSeparatedBySpaces_WhenVisibilityHasMultipleFlagsSet() {
    var sut = SyntaxVisibility.Internal | SyntaxVisibility.Protected;

    var actual = sut.ToVisibilityString();

    actual.Should().Be("internal protected");
  }
}