using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Application;
using CleanArchitectureApplication.Domain;
using CleanArchitectureApplication.Persistence;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;
using CleanArchitectureApplication.Presentation;

namespace CleanArchitectureApplication.ApiHost;

public partial interface IAppModule : IModule {
  public IDomainModule DomainModule { get; }
  public IApplicationModule ApplicationModule { get; }
  public IPersistenceModule PersistenceModule { get; }
  public IPresentationModule PresentationModule { get; }

  public IOrderingComponentComposite Ordering { get; }
}

[Entrypoint]
[Import<IDomainModule>]
[Import<IApplicationModule>]
[Import<IPersistenceModule>]
[Import<IPresentationModule>]
public partial interface IAppModule { }