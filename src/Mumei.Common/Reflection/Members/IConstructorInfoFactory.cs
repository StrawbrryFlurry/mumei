using System.Reflection;

namespace Mumei.Common.Reflection;

internal interface IConstructorInfoFactory {
  public ConstructorInfo CreateConstructorInfo(Type declaringType);
}