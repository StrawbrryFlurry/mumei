using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Application;
using CleanArchitectureApplication.Domain;
using CleanArchitectureApplication.Persistence;
using CleanArchitectureApplication.Presentation;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Module.Registration;

namespace CleanArchitectureApplication.ApiHost;

public partial interface IAppModule : IModule {
  public IDomainModule DomainModule { get; }
  public IApplicationModule ApplicationModule { get; }
  public IPersistenceModule PersistenceModule { get; }
  public IPresentationModule PresentationModule { get; }
}

[Entrypoint]
[Import<IDomainModule>]
[Import<IApplicationModule>]
[Import<IPersistenceModule>]
[Import<IPresentationModule>]
public partial interface IAppModule { }