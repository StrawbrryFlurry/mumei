using System.Reflection;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;

namespace Mumei.DependencyInjection.ServiceCollectionInterOpt.Provider;

public sealed class InjectorServiceProvider : IServiceProvider {
  private readonly IInjector _injector;
  private readonly IInjector? _scope;

  private static readonly MethodInfo EnumerableEmptyMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Empty))!;

  public InjectorServiceProvider(IInjector injector, IInjector? scope) {
    _injector = injector;
    _scope = scope;
  }

  public object? GetService(Type serviceType) {
    // We need to handle IEnumerable<T> specifically because
    // the service provider implementation does not care when
    // an enumerable service type does not have any registrations.
    // It will just return an empty enumerable of that type.
    var isEnumerableProvider = IsEnumerableProvider(serviceType);
    var injectFlags = isEnumerableProvider ? InjectFlags.Optional : InjectFlags.None;
    var result = _injector.Get(serviceType, _scope, injectFlags);

    return isEnumerableProvider
      ? (object?)result ?? MakeEmptyResultEnumerable(serviceType)
      : result;
  }

  private static bool IsEnumerableProvider(Type serviceType) {
    return serviceType.IsGenericType &&
           serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
  }

  private static object MakeEmptyResultEnumerable(Type serviceType) {
    var enumerableType = serviceType.GetGenericArguments()[0];
    var emptyEnumerableMethod = EnumerableEmptyMethod.MakeGenericMethod(enumerableType);

    return emptyEnumerableMethod.Invoke(null, null)!;
  }
}