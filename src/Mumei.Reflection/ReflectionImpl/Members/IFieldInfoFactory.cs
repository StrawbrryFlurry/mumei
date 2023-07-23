using System.Reflection;

namespace Mumei.Common;

internal interface IFieldInfoFactory {
  public FieldInfo CreateFieldInfo(Type declaringType);
}