using Mumei.DependencyInjection.Core;
using WeatherApplication;
using WeatherApplication.Generated;

namespace Mumei.Playground.SimpleApplication;

internal sealed class ApplicationModuleλApplicationEnvironment : ApplicationEnvironment<IApplicationModule> {
  public override TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    return Parent.Get<TProvider>(flags);
  }

  public override object Get(object token, InjectFlags flags = InjectFlags.None) {
    return Parent.Get(token, flags);
  }
}