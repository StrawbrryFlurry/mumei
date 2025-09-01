using System.Runtime.CompilerServices;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;
using Xunit.Sdk;

namespace Mumei.CodeGen.Qt.Tests.Setup;

[InterpolatedStringHandler]
internal readonly struct SyntaxVerificationExpectation {
    private readonly SyntaxWriter _writer;

    public SyntaxVerificationExpectation(int literalLength, int formattedCount) {
        _writer = new SyntaxWriter();
    }

    public void AppendLiteral(string s) {
        _writer.Write(s);
    }

    public void AppendFormatted(string s) {
        _writer.Write(s);
    }

    public void AppendFormatted(string s, string? format = null) {
        _writer.Write(s);
    }

    public void AppendFormatted(Type t, string? format = null!) {
        var writer = new SyntaxWriter();
        QtType.ForRuntimeType(t).WriteSyntax(ref writer, format);
        _writer.WriteFrom(writer);
    }

    public override string ToString() {
        return _writer.ToSyntax();
    }
}

internal sealed class SyntaxVerifier {
    public static void Verify<TRepresentable>(TRepresentable representable, SyntaxVerificationExpectation expected)
        where TRepresentable : ISyntaxRepresentable {
        var actual = representable.ToSyntaxInternal().TrimEnd();
        Verify(actual, expected);
    }

    public static void Verify(string actual, SyntaxVerificationExpectation expected) {
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
                 {expected}

                 Diff:
                 {diff}

                 Inner Exception:
                 {e}
                 """
            );
        }
    }


    public static void VerifyRegex<TRepresentable>(TRepresentable representable, SyntaxVerificationExpectation expected)
        where TRepresentable : ISyntaxRepresentable {

        var actual = representable.ToSyntaxInternal().TrimEnd();
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
        var result = new SyntaxWriter();
        foreach (var line in diff.Lines) {
            switch (line.Type) {
                case ChangeType.Inserted:
                    result.Write("+ ");
                    break;
                case ChangeType.Deleted:
                    result.Write("- ");
                    break;
                default:
                    result.Write("  ");
                    break;
            }

            result.WriteLine(line.Text);
        }

        return result.ToSyntax();
    }
}