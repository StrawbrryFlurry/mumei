namespace Mumei.CodeGen.Extensions;

public static class TypeExtensions {
  private const string AttributeSuffix = "Attribute";

  public static string GetAttributeName(this Type attributeType) {
    var name = attributeType.Name;

    if (name.EndsWith(AttributeSuffix)) {
      name = name.Substring(0, name.Length - AttributeSuffix.Length);
    }

    return name;
  }
}