using Xunit.Sdk;

namespace Mumei.CodeGen.Roslyn.Testing;

/// <summary>
/// Assertions for source texts which report failures as a line-by-line diff. Expected texts may contain placeholders:
/// <c>/*...*/</c> matches any text within a line and a line consisting only of <c>// ...</c> matches any number of lines.
/// </summary>
public static class SourceAssert {
    /// <summary>Asserts that <paramref name="actual"/> matches <paramref name="expected"/> in its entirety.</summary>
    /// <param name="sourceName">The name of the actual source (e.g. its file path) to include in failure messages.</param>
    public static void Equal(string expected, string actual, SourceComparisonOptions? options = null, string? sourceName = null) {
        ThrowIfNotMatch(SourceDiff.Compare(expected, actual, options), sourceName);
    }

    /// <summary>
    /// Asserts that <paramref name="actual"/> contains <paramref name="expectedFragment"/> as a contiguous block of lines.
    /// See <see cref="SourceDiff.CompareFragment"/>.
    /// </summary>
    /// <param name="sourceName">The name of the actual source (e.g. its file path) to include in failure messages.</param>
    public static void Contains(string expectedFragment, string actual, SourceComparisonOptions? options = null, string? sourceName = null) {
        ThrowIfNotMatch(SourceDiff.CompareFragment(expectedFragment, actual, options), sourceName);
    }

    private static void ThrowIfNotMatch(SourceDiffResult result, string? sourceName) {
        if (!result.IsMatch) {
            throw new XunitException(result.Format(sourceName));
        }
    }
}
