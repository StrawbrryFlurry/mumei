using System.Reflection;

namespace Mumei.Common.Reflection;

public interface IPropertyInfoFactory {
  public PropertyInfo CreatePropertyInfo(Type declaringType);
}