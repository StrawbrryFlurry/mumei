using Mumei.Core.Attributes;

namespace Mumei.Core;

public class TestModule : IModule {
  [Transient<IProvider, DynamicProvider>]
  [Transient<Provider>]
  public void Providers() { }

  public void Imports() { }

  public void Component() { }
}

public interface IProvider { }

[Injectable]
public class Provider : IProvider { }

[Injectable]
public class DynamicProvider : IDynamicProvider<Provider> {
  public Provider Provide() {
    return new Provider();
  }
}