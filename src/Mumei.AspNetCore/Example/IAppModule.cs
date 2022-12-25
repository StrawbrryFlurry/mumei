﻿using Mumei.AspNetCore.Example.Cats.Services;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace Mumei.AspNetCore.Example;

[RootModule]
public interface IAppModule {
  [Singleton<CatService>]
  ICatService CatService { get; }
}