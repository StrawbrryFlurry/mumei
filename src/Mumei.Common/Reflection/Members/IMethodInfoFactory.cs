using System.Reflection;

namespace Mumei.Common.Reflection;

internal interface IMethodInfoFactory {
  public MethodInfo CreateMethodInfo(Type declaringType);
}