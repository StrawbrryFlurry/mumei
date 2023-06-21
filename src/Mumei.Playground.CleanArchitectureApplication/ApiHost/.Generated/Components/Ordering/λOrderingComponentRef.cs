using System.Reflection;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λOrderingComponentRef : ComponentRef<IOrderingComponentComposite> {
  public λOrderingComponentRef(IInjector parent) : base(parent) { }

  public void Realize() {
    Injector = new λOrderingComponentInjector(Parent);
  }
}