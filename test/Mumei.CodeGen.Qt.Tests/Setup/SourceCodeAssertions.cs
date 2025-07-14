using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Qt.Tests.Setup;

internal static partial class SourceCodeAssertions {
    public static SyntaxTree HasFileMatching(
        this SourceGeneratorTestResult runResult,
        string fileRegex
    ) {
        var match = runResult.GeneratedTrees.FirstOrDefault(x => WildcardMatcher.Matches(x.FilePath, fileRegex))
                    ?? throw new Xunit.Sdk.XunitException(
                        $"Run Result did not contain a file matching '{fileRegex}'. Has: {string.Join("\n", runResult.GeneratedTrees.Select(x => x.FilePath))}");

        return match;
    }

    public static SyntaxTree WithPartialContent(
        this SyntaxTree syntaxTree,
        string partialContent
    ) {
        var content = syntaxTree.GetText().ToString();
        if (!WildcardMatcher.Matches(content, partialContent)) {
            throw new Xunit.Sdk.XunitException($"Did not find partial content:\n{partialContent}\n\nIn:\n{content}");
        }

        return syntaxTree;
    }
}