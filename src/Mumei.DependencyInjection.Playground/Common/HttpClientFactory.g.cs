﻿using Mumei.Core;
using Mumei.DependencyInjection.Playground.Common;
using HttpClient = Mumei.DependencyInjection.Playground.Common.HttpClient;

namespace Mumei.DependencyInjection.Playground;

public class HttpClientλFactory : IProviderFactory<IHttpClient> {
  public IHttpClient Get() {
    return new HttpClient();
  }
}