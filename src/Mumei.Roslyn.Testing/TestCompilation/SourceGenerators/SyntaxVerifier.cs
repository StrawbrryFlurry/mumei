using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Xunit;
using Xunit.Sdk;

namespace Mumei.Roslyn.Testing;

public static class SyntaxVerifier {
    public static void Verify(string actual, CommonSyntaxStringInterpolationHandler expected) {
        var expectedStr = expected.ToString();

        try {
            Assert.Equal(
                actual,
                expectedStr,
                ignoreWhiteSpaceDifferences: true,
                ignoreLineEndingDifferences: true
            );
        } catch (EqualException e) {
            var diff = Diff(actual, expectedStr);
            throw new XunitException(
                $"""
                 Syntax verification failed.
                 Actual:
                 {actual}

                 Expected:
                 {expectedStr}

                 Diff:
                 {diff}

                 Inner Exception:
                 {e}
                 """
            );
        }
    }

    public static void VerifyRegex(string actual, CommonSyntaxStringInterpolationHandler expected) {
        actual = actual.TrimEnd();
        var expectedString = expected.ToString();

        var doesMatch = WildcardMatcher.Matches(
            actual,
            expectedString
        );
        if (!doesMatch) {
            var result = Diff(actual, expectedString);

            throw new XunitException(
                $"""
                 Syntax verification failed.
                 Expected:
                 {expectedString}

                 Actual:
                 {actual}

                 Diff:
                 {result}
                 """
            );
        }
    }

    private static string Diff(string actual, string expected) {
        var diff = InlineDiffBuilder.Diff(actual, expected, true, true);
        var result = new StringBuilder();
        foreach (var line in diff.Lines) {
            switch (line.Type) {
                case ChangeType.Inserted:
                    result.Append("+ ");
                    break;
                case ChangeType.Deleted:
                    result.Append("- ");
                    break;
                default:
                    result.Append("  ");
                    break;
            }

            result.AppendLine(line.Text);
        }

        return result.ToString();
    }
}