using System.Diagnostics.CodeAnalysis;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class DynamicProviderBinder {
  public required RoslynType DynamicProviderType { get; init; }

  public static bool TryCreateFromAttribute(
    in RoslynAttribute attribute,
    [NotNullWhen(true)] out DynamicProviderBinder? dynamicProviderBinder
  ) {
    if (!attribute.IsConstructedGenericTypeOf(typeof(DynamicallyBindAttribute<>))) {
      dynamicProviderBinder = null!;
      return false;
    }

    dynamicProviderBinder = new DynamicProviderBinder {
      DynamicProviderType = attribute.Type.GetFirstTypeArgument()
    };
    return true;
  }
}