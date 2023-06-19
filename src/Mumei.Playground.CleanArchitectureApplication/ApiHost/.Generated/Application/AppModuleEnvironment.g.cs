using CleanArchitectureApplication;
using CleanArchitectureApplication.ApiHost;
using CleanArchitectureApplication.ApiHost.Generated;
using Mumei.DependencyInjection.Core;

namespace Mumei.Playground.SimpleApplication;

internal sealed class AppModuleλApplicationEnvironment : ApplicationEnvironment<IAppModule> {
  public override TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    return Parent.Get<TProvider>(flags);
  }

  public override object Get(object token, InjectFlags flags = InjectFlags.None) {
    return Parent.Get(token, flags);
  }
}