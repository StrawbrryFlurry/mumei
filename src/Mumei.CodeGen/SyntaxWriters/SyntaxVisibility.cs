using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.SyntaxWriters;

[Flags]
public enum SyntaxVisibility {
  None = 1 << 0,
  Public = 1 << 1,
  Private = 1 << 2,
  Internal = 1 << 3,
  Protected = 1 << 4,
  Abstract = 1 << 5,
  ReadOnly = 1 << 6,
  Static = 1 << 7,
  Virtual = 1 << 8,
  Override = 1 << 9,
  Sealed = 1 << 10,
  New = 1 << 11,
  Partial = 1 << 12,
  Extern = 1 << 13,
  Volatile = 1 << 14,
  Unsafe = 1 << 15,
  Required = 1 << 16,
  Async = 1 << 17
}

public enum DeclarationVisibility {
  Public = SyntaxVisibility.Public,
  Private = SyntaxVisibility.Private,
  Internal = SyntaxVisibility.Internal
}

public static class VisibilityExtensions {
  public static string ToVisibilityString(this SyntaxVisibility visibility) {
    if (visibility == SyntaxVisibility.None) {
      return "";
    }

    var flags = visibility.GetFlags();

    var visibilities = flags.Select(f => f.ToString().ToLower());
    var visibilityString = string.Join(" ", visibilities);

    return visibilityString;
  }

  public static string ToVisibilityString(this DeclarationVisibility visibility) {
    return visibility.ToString().ToLower();
  }
}