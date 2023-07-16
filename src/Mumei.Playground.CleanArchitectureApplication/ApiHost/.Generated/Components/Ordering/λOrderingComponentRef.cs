using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Module;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λOrderingComponentRef : ComponentRef<IOrderingComponentComposite> {
  public λOrderingComponentRef(IInjector parent) : base(parent) { }

  public void Realize() {
    Injector = new λOrderingComponentInjector(Parent);
  }
}