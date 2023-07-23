using System.Reflection;

namespace Mumei.Common; 

public static class CommonModuleAssemblyReference {
  public static Assembly Assembly { get; } = typeof(CommonModuleAssemblyReference).Assembly;
}