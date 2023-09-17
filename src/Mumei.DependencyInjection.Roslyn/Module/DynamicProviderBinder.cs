using System.Diagnostics.CodeAnalysis;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class DynamicProviderBinder {
  public ModuleDeclaration Parent { get; }

  public static bool TryCreateFromAttribute(
    CompilationAttribute attribute,
    [NotNullWhen(true)] out DynamicProviderBinder? dynamicProviderBinder
  ) {
    var attributeType = attribute.GetType();
    if (attributeType.GetGenericTypeDefinition() == typeof(DynamicallyBindAttribute<>)) {
      dynamicProviderBinder = null!; // attributeType.GetGenericArguments()[0];
      return true;
    }

    dynamicProviderBinder = null!;
    return false;
  }
}