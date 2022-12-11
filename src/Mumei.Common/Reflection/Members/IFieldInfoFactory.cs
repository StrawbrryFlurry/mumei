using System.Reflection;

namespace Mumei.Common.Reflection;

internal interface IFieldInfoFactory {
  public FieldInfo CreateFieldInfo(Type declaringType);
}