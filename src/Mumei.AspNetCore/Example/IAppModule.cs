using Mumei.AspNetCore.Example.Cats.Services;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;

namespace Mumei.AspNetCore.Example;

[RootModule]
public interface IAppModule : IModule {
  [Singleton<CatService>]
  ICatService CatService { get; }
}