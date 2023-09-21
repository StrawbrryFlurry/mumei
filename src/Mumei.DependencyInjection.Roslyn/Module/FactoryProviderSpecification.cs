using System.Reflection;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

public class FactoryProviderSpecification {
  public static bool TryCreateFromMethod(
    in RoslynMethodInfo method,
    out FactoryProviderSpecification o
  ) {
    throw new NotImplementedException();
  }
}