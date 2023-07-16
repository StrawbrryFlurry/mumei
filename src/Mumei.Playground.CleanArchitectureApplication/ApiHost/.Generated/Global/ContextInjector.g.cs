using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;

namespace CleanArchitectureApplication.ApiHost.Generated; 

public sealed class ContextInjector : IInjector {
  public IInjector Parent { get; }

  public required HttpContext Context { get; init; }

  public ContextInjector(IInjector parent) {
    Parent = parent;
  }
  
  public TProvider Get<TProvider>(IInjector? scope, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector scope, InjectFlags flags = InjectFlags.None) {
    return token switch {
      _ when token == typeof(HttpContext) => Context,
      _ => Parent.Get(token, scope, flags)
    };
  }
}