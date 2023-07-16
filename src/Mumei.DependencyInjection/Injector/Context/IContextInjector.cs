using Mumei.DependencyInjection.Injector.Registration;

namespace Mumei.DependencyInjection.Injector.Context;

/// <summary>
/// Declares an injector as a context injector.
/// If a context injector is passed to an injector as a scope,
/// that context will be used to resolve providers with the <see cref="ProvidedIn.Context"/>
/// scope. 
/// </summary>
public interface IContextInjector {
  public bool TryGet(object token, IInjector scope, out object? provider);
}