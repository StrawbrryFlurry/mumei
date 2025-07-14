using System.Text.RegularExpressions;

namespace Mumei.CodeGen.Qt.Tests.Setup;

public static partial class WildcardMatcher {
    public static bool Matches(string text, string pattern, bool ignoreWhitespace = true) {
        text = NormalizeWhitespace(text);
        pattern = NormalizeWhitespace(pattern);

        if (ignoreWhitespace) {
            text = NormalizeWhitespaceRegex().Replace(text, "");
            pattern = NormalizeWhitespaceRegex().Replace(pattern, "");
        }

        var regexPattern = Regex.Escape(pattern)
            .Replace("\\*", ".*?") // * matches any characters
            .Replace("\\?", ".") // ? matches single character
            .Replace("\\[ANY]", ".*?"); // [ANY] matches any characters

        if (ignoreWhitespace) {
            regexPattern = NormalizeWhitespaceRegex().Replace(regexPattern, @"\s+");
            regexPattern = NormalizeLineEndingsRegex().Replace(regexPattern, @"\n+");
        }

        return Regex.IsMatch(text, regexPattern, RegexOptions.Singleline);
    }

    private static string NormalizeWhitespace(string input) {
        return NormalizeWhitespaceRegex().Replace(input, " ").Trim();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex NormalizeWhitespaceRegex();

    private static string NormalizeLineEndings(string input) {
        return NormalizeLineEndingsRegex().Replace(input, "\n");
    }

    [GeneratedRegex(@"\r\n?|\n")]
    private static partial Regex NormalizeLineEndingsRegex();
}