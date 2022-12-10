using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.Playground.Example.Generated;

internal sealed class ApplicationModuleλEnvironmentInjector : EnvironmentInjector<IApplicationModule> {
  public override TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    return Parent.Get<TProvider>(flags);
  }

  public override object Get(object token, InjectFlags flags = InjectFlags.None) {
    return Parent.Get(token, flags);
  }
}