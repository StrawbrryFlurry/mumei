using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class MumeiCustomAttributeProvider : ICustomAttributeProvider {
  private readonly Type _type;

  public MumeiCustomAttributeProvider(Type type) {
    _type = type;
  }

  public object[] GetCustomAttributes(bool inherit) {
    return _type.GetCustomAttributes(inherit);
  }

  public object[] GetCustomAttributes(Type attributeType, bool inherit) {
    return _type.GetCustomAttributes(attributeType, inherit);
  }

  public bool IsDefined(Type attributeType, bool inherit) {
    return _type.IsDefined(attributeType, inherit);
  }
}