using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;

namespace Mumei.AspNetCore.Example.Generated;

internal partial class AppModuleλEnvironmentInjector : EnvironmentInjector<IAppModule> {
  public override TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return Parent.Get<TProvider>(scope, flags);
  }

  public override object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return Parent.Get(token, scope, flags);
  }
}