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
            var diff = Diff(actual, expectedStr, false);
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
            var result = Diff(actual, expectedString, true);

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

    private static string Diff(string actual, string expected, bool ignoreRegex) {
        var diff = InlineDiffBuilder.Diff(actual, expected, true, true);
        var result = new StringBuilder();

        var lineIdx = 0;
        while (lineIdx < diff.Lines.Count) {
            var currentLine = diff.Lines[lineIdx];
            if (currentLine.Type == ChangeType.Unchanged) {
                goto WriteThisLine;
            }

            // Ignore changes if they are wildcard differences
            if (!ignoreRegex) {
                goto WriteThisLine;
            }

            if (currentLine.Type == ChangeType.Deleted && lineIdx > 0 && diff.Lines[lineIdx - 1].Type == ChangeType.Unchanged) {
                if (lineIdx + 1 < diff.Lines.Count && diff.Lines[lineIdx + 1].Type == ChangeType.Inserted) {
                    var deletedLine = currentLine.Text;
                    var insertedLine = diff.Lines[lineIdx + 1].Text;
                    var isWildcardChange = WildcardMatcher.Matches(deletedLine, insertedLine);
                    if (isWildcardChange) {
                        lineIdx += 1;
                        currentLine = diff.Lines[lineIdx + 1];
                        currentLine.Type = ChangeType.Unchanged;
                    }
                }
            }

            WriteThisLine:
            switch (currentLine.Type) {
                case ChangeType.Inserted:
                    result.Append("\x1b[32m");
                    result.Append(currentLine.Text);
                    result.AppendLine("\x1b[0m");
                    break;
                case ChangeType.Deleted:
                    result.Append("\x1b[31m");
                    result.Append(currentLine.Text);
                    result.AppendLine("\x1b[0m");
                    break;
                default:
                    result.AppendLine(currentLine.Text);
                    break;
            }

            lineIdx++;
        }

        return result.ToString();
    }
}