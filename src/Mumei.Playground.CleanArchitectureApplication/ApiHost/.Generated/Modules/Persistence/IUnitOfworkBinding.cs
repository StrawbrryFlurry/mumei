using CleanArchitectureApplication.Domain.Common.Persistence;
using CleanArchitectureApplication.Persistence.Common;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λIUnitOfworkBinding : ScopedBinding<IUnitOfWork> {
  protected override IUnitOfWork Create(IInjector? scope = null) {
    return new UnitOfWork();
  }
}