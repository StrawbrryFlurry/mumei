using Mumei.DependencyInjection.Injector.Registration;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal interface IProviderSpec {
  public RoslynType ProviderType { get; }
}