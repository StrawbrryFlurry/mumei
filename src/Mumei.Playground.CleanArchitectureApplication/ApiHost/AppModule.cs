using CleanArchitectureApplication.Application;
using CleanArchitectureApplication.Domain;
using CleanArchitectureApplication.Persistence;
using CleanArchitectureApplication.Presentation;
using Mumei.DependencyInjection.Attributes;

namespace CleanArchitectureApplication.ApiHost;

[Entrypoint]
[Import<IDomainModule>]
[Import<IApplicationModule>]
[Import<IPersistenceModule>]
[Import<IPresentationModule>]
public partial interface IAppModule { }