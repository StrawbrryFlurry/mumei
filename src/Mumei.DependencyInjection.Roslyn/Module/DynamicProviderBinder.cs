using System.Diagnostics.CodeAnalysis;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class DynamicProviderBinder {
  public ModuleDeclaration Parent { get; }

  public static bool TryCreateFromAttribute(
    in RoslynAttribute attribute,
    [NotNullWhen(true)] out DynamicProviderBinder? dynamicProviderBinder
  ) {
    if (attribute.IsConstructedGenericTypeOf(typeof(DynamicallyBindAttribute<>))) {
      dynamicProviderBinder = null!; // attributeType.GetGenericArguments()[0];
      return true;
    }

    dynamicProviderBinder = null!;
    return false;
  }
}