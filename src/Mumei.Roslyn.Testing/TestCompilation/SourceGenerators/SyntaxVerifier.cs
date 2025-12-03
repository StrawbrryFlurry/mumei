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
            var (fullDiff, minimalDiff) = Diff(actual, expectedStr, true);

            throw new XunitException(
                $"""
                 Syntax verification failed.
                 {minimalDiff}

                 Expected:
                 {expectedStr}

                 Actual:
                 {actual}

                 Full Diff:
                 {fullDiff}

                 Inner Exception:
                 {e}
                 """
            );
        }
    }

    public static void VerifyRegex(string actual, CommonSyntaxStringInterpolationHandler expected) {
        actual = actual.TrimEnd();
        var expectedStr = expected.ToString();

        var doesMatch = WildcardMatcher.Matches(
            actual,
            expectedStr
        );
        if (!doesMatch) {
            var (fullDiff, minimalDiff) = Diff(actual, expectedStr, true);

            throw new XunitException(
                $"""
                 Syntax verification failed.
                 {minimalDiff}

                 Expected:
                 {expectedStr}

                 Actual:
                 {actual}

                 Full Diff:
                 {fullDiff}
                 """
            );
        }
    }

    private static (string FullDiff, string MinimalDiff) Diff(string actual, string expected, bool ignoreRegex) {
        var diff = InlineDiffBuilder.Diff(actual, expected, false, false);
        var minimalDiff = new StringBuilder();
        var fullDiff = new StringBuilder();

        var lineIdx = 0;
        var lines = diff.Lines;
        while (lineIdx < lines.Count) {
            var currentLine = lines[lineIdx];
            if (currentLine.Type == ChangeType.Unchanged) {
                goto WriteThisLine;
            }

            // Ignore changes if they are wildcard differences
            if (!ignoreRegex) {
                goto WriteThisLine;
            }

            if (currentLine.Type == ChangeType.Deleted && lineIdx > 0 && lines[lineIdx - 1].Type == ChangeType.Unchanged) {
                if (lineIdx + 1 < lines.Count && lines[lineIdx + 1].Type == ChangeType.Inserted) {
                    var deletedLine = currentLine.Text;
                    var insertedLine = lines[lineIdx + 1].Text;
                    var isWildcardChange = WildcardMatcher.Matches(deletedLine, insertedLine);
                    if (isWildcardChange) {
                        lineIdx += 1;
                        currentLine = lines[lineIdx + 1];
                        currentLine.Type = ChangeType.Unchanged;
                    }
                }
            }

            WriteThisLine:
            switch (currentLine.Type) {
                case ChangeType.Inserted:
                    AppendCurrentDiffLine("\x1b[32m", "\x1b[0m");
                    break;
                case ChangeType.Deleted:
                    AppendCurrentDiffLine("\x1b[31m", "\x1b[0m");
                    break;
                default:
                    fullDiff.AppendLine(currentLine.Text);
                    break;
            }

            lineIdx++;

            void AppendCurrentDiffLine(string prefix, string suffix) {
                var isStartOfNewDiffBlock = lineIdx == 0 || lineIdx > 0 && lines[lineIdx - 1].Type == ChangeType.Unchanged;
                if (isStartOfNewDiffBlock && minimalDiff.Length > 0) {
                    minimalDiff.AppendLine("...");
                }

                var appendContext = lineIdx > 0 && isStartOfNewDiffBlock;
                if (appendContext) {
                    minimalDiff.Append(lines[lineIdx - 1]);
                }

                minimalDiff.Append(prefix);
                minimalDiff.Append(currentLine.Text);
                minimalDiff.AppendLine(suffix);

                var appendFollowingContext = lineIdx + 1 < lines.Count && lines[lineIdx + 1].Type == ChangeType.Unchanged;
                if (appendFollowingContext) {
                    minimalDiff.Append(lines[lineIdx + 1]);
                }

                fullDiff.Append(prefix);
                fullDiff.Append(currentLine.Text);
                fullDiff.AppendLine(suffix);
            }
        }

        return (fullDiff.ToString(), minimalDiff.ToString());
    }
}