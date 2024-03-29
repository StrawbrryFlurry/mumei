﻿using CleanArchitectureApplication.Presentation.Ordering;
using Mumei.DependencyInjection.Module;

namespace CleanArchitectureApplication.Presentation;

public partial interface IPresentationModule : IModule {
  public IOrderComponent Ordering { get; }
}

[Component<IOrderComponent>]
public partial interface IPresentationModule { }