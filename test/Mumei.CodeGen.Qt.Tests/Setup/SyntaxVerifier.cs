using System.Runtime.CompilerServices;
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
        QtType.ForRuntimeType(t).WriteSyntax(writer, format);
        _writer.WriteFrom(writer);
    }

    public override string ToString() {
        return _writer.ToSyntax();
    }
}

internal sealed class SyntaxVerifier {
    public static void Verify<TRepresentable>(TRepresentable representable, SyntaxVerificationExpectation expected)
        where TRepresentable : ISyntaxRepresentable {
        try {
            Assert.Equal(
                representable.ToSyntaxInternal().TrimEnd(),
                expected.ToString(),
                ignoreWhiteSpaceDifferences: true,
                ignoreLineEndingDifferences: true
            );
        }
        catch (EqualException e) {
            throw new XunitException(
                $"""
                 Syntax verification failed.
                 Expected:
                 {expected}

                 Actual:
                 {representable.ToSyntaxInternal()}

                 {e}
                 """
            );
        }
    }
}