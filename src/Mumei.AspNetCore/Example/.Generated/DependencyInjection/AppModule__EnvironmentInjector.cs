using Mumei.DependencyInjection.Core;

namespace Mumei.AspNetCore.Example.Generated;

internal partial class AppModuleλEnvironmentInjector : EnvironmentInjector<IAppModule> {
  public override TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    return Parent.Get<TProvider>(flags);
  }

  public override object Get(object token, InjectFlags flags = InjectFlags.None) {
    return Parent.Get(token, flags);
  }
}