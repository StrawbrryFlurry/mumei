using System.Reflection;

namespace Mumei.Common;

public interface IPropertyInfoFactory {
  public PropertyInfo CreatePropertyInfo(Type declaringType);
}