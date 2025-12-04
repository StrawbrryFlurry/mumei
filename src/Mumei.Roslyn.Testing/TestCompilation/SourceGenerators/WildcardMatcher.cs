using System.Text.RegularExpressions;

namespace Mumei.Roslyn.Testing;

internal static class WildcardMatcher {
    public static bool Matches(string text, string pattern) {
        text = TrimEachLine(text.ReplaceLineEndings("\n"));
        pattern = TrimEachLine(pattern.ReplaceLineEndings("\n"));

        var regexPattern = Regex.Escape(pattern)
            .Replace("\\*", ".*?") // * matches any characters
            .Replace("\\?", ".") // ? matches single character
            .Replace("\\[ANY\\]", ".*?"); // [ANY] matches any characters

        return Regex.IsMatch(text, regexPattern, RegexOptions.Singleline);
    }

    private static string TrimEachLine(string text) {
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++) {
            lines[i] = lines[i].Trim();
        }

        return string.Join("\n", lines);
    }
}