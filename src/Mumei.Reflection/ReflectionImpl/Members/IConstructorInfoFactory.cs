using System.Reflection;

namespace Mumei.Common;

internal interface IConstructorInfoFactory {
  public ConstructorInfo CreateConstructorInfo(Type declaringType);
}