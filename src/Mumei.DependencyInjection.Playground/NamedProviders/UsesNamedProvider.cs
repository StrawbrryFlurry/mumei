using Mumei.DependencyInjection.Attributes;

namespace Mumei.DependencyInjection.Playground.NamedProviders;

public class UsesNamedProvider {
  private INamedProvider _namedProvider;

  public UsesNamedProvider([Inject("NamedProvider")] INamedProvider namedProvider) {
    _namedProvider = namedProvider;
  }
}