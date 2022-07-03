using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.SyntaxWriters;

[Flags]
public enum MemberVisibility {
  Public = 1 << 0,
  Private = 1 << 1,
  Internal = 1 << 2,
  Protected = 1 << 3
}

public enum TypeDeclarationVisibility {
  Public = MemberVisibility.Public,
  Internal = MemberVisibility.Internal
}

public static class VisibilityExtensions {
  public static string ToVisibilityString(this MemberVisibility visibility) {
    var flags = visibility.GetFlags();

    var visibilities = flags.Select(f => f.ToString().ToLower());
    var visibilityString = string.Join(" ", visibilities);

    return visibilityString;
  }

  public static string ToVisibilityString(this TypeDeclarationVisibility visibility) {
    return visibility.ToString().ToLower();
  }
}