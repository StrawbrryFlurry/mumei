using Mumei.DependencyInjection.Injector.Registration;
using Mumei.DependencyInjection.Injector.Resolution;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Dynamic;
using Mumei.DependencyInjection.Providers.Dynamic.Resolution;
using Mumei.DependencyInjection.Providers.Registration;

namespace Mumei.DependencyInjection.Injector.Implementation;

public abstract class DynamicInjector {
  private readonly IInjector _injector;
  private readonly Dictionary<object, Binding> _bindings = new();
  private readonly ProviderCollection _providers = new();

  public IReadOnlyCollection<ProviderDescriptor> Providers => _providers;

  public DynamicInjector(
    IInjector injector,
    Action<ProviderCollection> providerBinder
  ) {
    _injector = injector;
    ConfigureProviders(providerBinder);
  }

  private void ConfigureProviders(Action<ProviderCollection> providerBinder) {
    providerBinder(_providers);

    // ToList allows us to iterate over a copy of the collection while adding to it.
    foreach (var provider in _providers.ToList()) {
      // In generated injectors, bindings are automatically added as providers.
      // Here, we need to manually do that work after the provider collection
      // has been configured.
      AddProviderBindingAsProvider(provider);
    }
  }

  private void AddProviderBindingAsProvider(ProviderDescriptor provider) {
    var providerType = GetProviderType(provider);
    var genericProviderType = typeof(Binding<>).MakeGenericType(providerType);

    _providers.Add(new ProviderDescriptor {
      Lifetime = InjectorLifetime.Scoped,
      Token = genericProviderType,
      // TODO: Can we somehow avoid capturing `provider` in a closure?
      ImplementationFactory = (_, _) => MakeDynamicBinding(provider)
    });
  }

  public bool TryGet(object token, IInjector scope, out object provider) {
    if (_bindings.TryGetValue(token, out var binding)) {
      provider = binding.GetInstance(scope);
      return true;
    }

    if (!_providers.TryGet(token, out var descriptor)) {
      provider = default!;
      return false;
    }

    binding = MakeDynamicBinding(descriptor!);
    provider = binding.GetInstance(scope);
    return true;
  }

  private Binding MakeDynamicBinding(ProviderDescriptor descriptor) {
    var token = descriptor.Token;
    // This avoids capturing the descriptor as a closure
    var providerFactory = MakeProviderFactoryForDescriptor(descriptor);
    var bindingType = GetProviderType(descriptor);

    Binding binding;
    if (descriptor.Lifetime is InjectorLifetime.Scoped) {
      binding = DynamicScopedBinding.CreateDynamic(bindingType, providerFactory, _injector);
      _bindings.Add(token, binding);
      return binding;
    }

    if (descriptor.Lifetime is InjectorLifetime.Transient) {
      binding = DynamicTransientBinding.CreateDynamic(bindingType, providerFactory, _injector);
      _bindings.Add(token, binding);
      return binding;
    }

    var singletonInstance = providerFactory(_injector);
    binding = DynamicSingletonBinding.CreateDynamic(bindingType, singletonInstance);
    _bindings.Add(token, binding);
    return binding;
  }

  private static DynamicProviderFactory MakeProviderFactoryForDescriptor(
    ProviderDescriptor descriptor
  ) {
    // TODO: Get rid of `descriptor` closures
    if (descriptor.ImplementationInstance is not null) {
      return (_, _) => descriptor.ImplementationInstance!;
    }

    if (descriptor.ImplementationFactory is not null) {
      return descriptor.ImplementationFactory!;
    }

    if (descriptor.ImplementationType is not null) {
      return (injector, scope) => InjectorTypeActivator.CreateInstance(
        descriptor.ImplementationType!,
        injector, scope
      );
    }

    throw new InvalidOperationException();
  }

  private static Type GetProviderType(ProviderDescriptor provider) {
    if (provider.Token is Type providerType) {
      return providerType;
    }

    var staticallyKnownType = provider.ImplementationType ?? provider.ImplementationInstance?.GetType();
    if (staticallyKnownType is not null) {
      return staticallyKnownType;
    }

    var factoryType = provider.ImplementationFactory?.GetType()! ?? throw new InvalidOperationException();
    var factoryResultType = factoryType.GetGenericArguments()[1];

    if (factoryResultType != typeof(object)) {
      return factoryResultType;
    }

    throw new InvalidOperationException($"Could not determine provider type for provider {provider}.");
  }
}