using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Resolution;
using Mumei.DependencyInjection.Providers;

namespace Mumei.DependencyInjection.Playground.NamedProviders;

public class UsesNamedProvider {
  private INamedProvider _namedProvider;

  public UsesNamedProvider([Inject("NamedProvider")] INamedProvider namedProvider) {
    _namedProvider = namedProvider;
  }
}