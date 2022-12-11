﻿using Mumei.DependencyInjection.Core;

namespace Mumei.AspNetCore.Common.Application;

public interface IMumeiWebApplicationBuilder {
  public IInjector Injector { get; }
  public IServiceCollection Services { get; }
  public IMumeiWebApplication Build();
}