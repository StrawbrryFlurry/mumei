using System.Reflection;

namespace Mumei.Common;

internal interface IMethodInfoFactory {
  public MethodInfo CreateMethodInfo(Type declaringType);
}