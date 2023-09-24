using Mumei.DependencyInjection.Providers.Registration;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

public ref struct MultiProviderScopeCollector {
  public static MultiProviderScope CollectFromAttribute(RoslynAttribute attribute) {
    return attribute.GetArgument<MultiProviderScope>(nameof(MultiAttribute.Scope), 0);
  }
}