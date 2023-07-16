using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;
using WeatherApplication;
using WeatherApplication.Generated;

namespace Mumei.Playground.SimpleApplication;

internal sealed class ApplicationModuleλApplicationEnvironment : ApplicationEnvironment<IApplicationModule> {
  public override TProvider Get<TProvider>(IInjector? scope, InjectFlags flags = InjectFlags.None) {
    return Parent.Get<TProvider>(scope, flags);
  }

  public override object Get(object token, IInjector? scope, InjectFlags flags = InjectFlags.None) {
    return Parent.Get(token, scope, flags);
  }
}