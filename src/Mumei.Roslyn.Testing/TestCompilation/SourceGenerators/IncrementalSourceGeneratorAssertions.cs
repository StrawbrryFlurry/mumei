using Microsoft.CodeAnalysis;
using Xunit.Sdk;

namespace Mumei.Roslyn.Testing;

public static class IncrementalSourceGeneratorAssertions {
    extension<TGenerator>(IncrementalSourceGeneratorTest<TGenerator>.Result result) where TGenerator : IIncrementalGenerator {
        public SyntaxTreeAssertion HasFileMatching(
            string fileRegex
        ) {
            var match = result.GeneratedTrees.FirstOrDefault(x => WildcardMatcher.Matches(x.FilePath, fileRegex))
                        ?? throw new XunitException(
                            $"Run Result did not contain a file matching '{fileRegex}'. Has:\n{string.Join("\n", result.GeneratedTrees.Select(x => $"- {x.FilePath}"))}");

            return new SyntaxTreeAssertion(match);
        }
    }

    public readonly struct SyntaxTreeAssertion(SyntaxTree syntaxTree) {
        public SyntaxTreeAssertion WithPartialContent(
            CommonSyntaxStringInterpolationHandler expectation
        ) {
            var content = syntaxTree.GetText().ToString();
            SyntaxVerifier.VerifyRegex(content, expectation);
            return this;
        }

        public SyntaxTreeAssertion WithContent(CommonSyntaxStringInterpolationHandler expectation) {
            var content = syntaxTree.GetText().ToString();
            SyntaxVerifier.Verify(content, expectation);
            return this;
        }
    }
}