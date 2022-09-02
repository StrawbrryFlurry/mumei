using Mumei.Core.Attributes;

namespace Mumei.DependencyInjection.Playground.Example.NamedProviders;

public class UsesNamedProvider {
  private INamedProvider _namedProvider;

  public UsesNamedProvider([Inject("NamedProvider")] INamedProvider namedProvider) {
    _namedProvider = namedProvider;
  }
}