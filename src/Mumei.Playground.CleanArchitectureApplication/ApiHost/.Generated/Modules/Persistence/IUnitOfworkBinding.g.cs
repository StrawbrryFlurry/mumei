using CleanArchitectureApplication.Domain.Common.Persistence;
using CleanArchitectureApplication.Persistence.Common;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Resolution;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λIUnitOfworkBinding : ScopedBinding<IUnitOfWork> {
  protected override IUnitOfWork Create(IInjector? scope = null) {
    return new UnitOfWork();
  }
}