using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Tests.Utils;

public static class StringExtensions {
  /// <summary>
  ///   Appends the Environment NewLine character sequence
  ///   to the end of the string
  /// </summary>
  public static string Nl(this string str) {
    return str += Environment.NewLine;
  }

  /// <summary>
  ///   Creates a string from the given array of strings
  ///   appending the Environment NewLine character sequence
  ///   at the end
  /// </summary>
  /// <param name="strings"></param>
  /// <returns></returns>
  public static string Line(params string[] strings) {
    var line = string.Join("", strings);
    return Nl(line);
  }

  public static string IndentedLine(string str, int indents = 0) {
    return Line(IndentationString(indents), str);
  }

  public static string IndentationString(int indents) {
    return new string(SyntaxWriter.IndentChar, SyntaxWriter.IndentSpacing * indents);
  }

  public static string IndentationString(string str, int indents) {
    return $"{IndentationString(indents)}{str}";
  }
}