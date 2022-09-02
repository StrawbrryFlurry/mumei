using Mumei.Core;
using Mumei.Core.Injector;

namespace Mumei.DependencyInjection.Playground.Generated;

public class EnvironmentInjector : IInjector {
  public static readonly EnvironmentInjector Instance = new();
  public IInjector Parent { get; } = NullInjector.Instance;

  public T Get<T>() {
    return Parent.Get<T>();
  }

  public object Get(Type provider) {
    return Parent.Get(provider);
  }
}