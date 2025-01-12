using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal interface IProviderDeclaration {
  public RoslynType ProviderType { get; }
}