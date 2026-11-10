using Microsoft.CodeAnalysis;
using Xunit.Sdk;

namespace Mumei.CodeGen.Roslyn.Testing;

public static class IncrementalSourceGeneratorAssertions {
    extension(GeneratorTest.Result result) {
        /// <summary>
        /// Finds the single generated file whose file name (or path) matches <paramref name="filePattern"/>,
        /// where <c>*</c> matches any text, e.g. <c>*.Assert.g.cs</c>.
        /// </summary>
        public SyntaxTreeAssertion HasFileMatching(string filePattern) {
            var matches = result.GeneratedTrees.Where(x => FilePatternMatches(filePattern, x.FilePath)).ToArray();
            return matches.Length switch {
                1 => new SyntaxTreeAssertion(matches[0]),
                0 => throw new XunitException(
                    $"No generated file matches '{filePattern}'. Generated files:\n{DescribeFiles(result.GeneratedTrees)}"
                ),
                _ => throw new XunitException(
                    $"Multiple generated files match '{filePattern}', use a more specific pattern. Matching files:\n{DescribeFiles(matches)}"
                )
            };
        }
    }

    private static bool FilePatternMatches(string pattern, string path) {
        path = path.Replace('\\', '/');
        pattern = pattern.Replace('\\', '/');
        return GlobMatches(pattern, Path.GetFileName(path)) || GlobMatches(pattern, path) || GlobMatches($"*/{pattern}", path);
    }

    private static bool GlobMatches(string pattern, string text) {
        var parts = pattern.Split('*');
        if (parts.Length == 1) {
            return string.Equals(pattern, text, StringComparison.Ordinal);
        }

        if (!text.StartsWith(parts[0], StringComparison.Ordinal)) {
            return false;
        }

        var position = parts[0].Length;
        for (var i = 1; i < parts.Length - 1; i++) {
            var index = text.IndexOf(parts[i], position, StringComparison.Ordinal);
            if (index < 0) {
                return false;
            }

            position = index + parts[i].Length;
        }

        return text.Length - position >= parts[^1].Length && text.EndsWith(parts[^1], StringComparison.Ordinal);
    }

    private static string DescribeFiles(IEnumerable<SyntaxTree> trees) {
        var lines = trees.Select(x => $"  - {x.FilePath} ({x.GetText().Lines.Count} lines)").ToArray();
        return lines.Length == 0 ? "  (none)" : string.Join("\n", lines);
    }

    public readonly struct SyntaxTreeAssertion(SyntaxTree syntaxTree) {
        public SyntaxTree SyntaxTree => syntaxTree;

        /// <summary>
        /// Asserts that the file contains <paramref name="expectation"/> as a contiguous block of lines.
        /// <c>/*...*/</c> matches any text within a line and a line consisting only of <c>// ...</c> matches any number of lines.
        /// </summary>
        public SyntaxTreeAssertion WithPartialContent(CodeFragment expectation, SourceComparisonOptions? options = null) {
            SourceAssert.Contains(expectation.ToString(), syntaxTree.GetText().ToString(), options, syntaxTree.FilePath);
            return this;
        }

        /// <summary>
        /// Asserts that the file content matches <paramref name="expectation"/>.
        /// <c>/*...*/</c> matches any text within a line and a line consisting only of <c>// ...</c> matches any number of lines.
        /// </summary>
        public SyntaxTreeAssertion WithContent(CodeFragment expectation, SourceComparisonOptions? options = null) {
            SourceAssert.Equal(expectation.ToString(), syntaxTree.GetText().ToString(), options, syntaxTree.FilePath);
            return this;
        }
    }
}
