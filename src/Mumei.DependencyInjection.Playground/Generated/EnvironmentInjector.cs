using Mumei.Core;
using Mumei.Core.Injector;

namespace Mumei.DependencyInjection.Playground.Generated;

public class EnvironmentInjector : IInjector {
  public static readonly EnvironmentInjector Instance = new();
  public IInjector Parent { get; } = NullInjector.Instance;

  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    return Parent.Get<TProvider>();
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    return Parent.Get(token, flags);
  }
}