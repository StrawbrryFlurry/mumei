using FluentAssertions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxWriters;

public class SyntaxVisibilityTest {
  [Fact]
  public void ToVisibilityString_ReturnsSingleVisibilityAsString() {
    var sut = SyntaxVisibility.Public;

    var actual = sut.ToVisibilityString();

    actual.Should().Be("public");
  }

  [Fact]
  public void ToVisibilityString_ReturnsAllVisibilitiesSeparatedBySpaces_WhenVisibilityHasMultipleFlagsSet() {
    var sut = SyntaxVisibility.Internal | SyntaxVisibility.Protected;

    var actual = sut.ToVisibilityString();

    actual.Should().Be("internal protected");
  }

  [Fact]
  public void ToVisibilityString_ReturnsEmptyString_WhenVisibilityIsNone() {
    var sut = SyntaxVisibility.None;

    var actual = sut.ToVisibilityString();

    actual.Should().BeEmpty();
  }
}