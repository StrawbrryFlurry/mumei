namespace Mumei.DependencyInjection.Core;

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
      Lifetime = InjectorLifetime.Singleton,
      Token = genericProviderType,
      // TODO: Can we somehow avoid capturing `provider` in a closure?
      ImplementationFactory = _ => MakeDynamicBinding(provider)
    });
  }

  public bool TryGet(object token, out object provider) {
    if (_bindings.TryGetValue(token, out var binding)) {
      provider = binding.GetInstance(_injector);
      return true;
    }

    if (!_providers.TryGet(token, out var descriptor)) {
      provider = default!;
      return false;
    }

    binding = MakeDynamicBinding(descriptor!);
    provider = binding.GetInstance(_injector);
    return true;
  }

  private Binding MakeDynamicBinding(ProviderDescriptor descriptor) {
    var token = descriptor.Token;
    var factory = MakeDescriptorFactory(descriptor);
    var bindingType = GetProviderType(descriptor);

    Binding binding;
    if (descriptor.Lifetime is InjectorLifetime.Scoped) {
      binding = DynamicScopedBinding.CreateDynamic(bindingType, factory(descriptor), _injector);
      _bindings.Add(token, binding);
      return binding;
    }

    if (descriptor.Lifetime is InjectorLifetime.Transient) {
      binding = DynamicTransientBinding.CreateDynamic(bindingType, factory(descriptor), _injector);
      _bindings.Add(token, binding);
      return binding;
    }

    binding = DynamicSingletonBinding.CreateDynamic(bindingType, factory(descriptor)(_injector));
    _bindings.Add(token, binding);
    return binding;
  }

  private static Func<ProviderDescriptor, Func<IInjector, object>> MakeDescriptorFactory(
    ProviderDescriptor descriptor
  ) {
    if (descriptor.ImplementationInstance is not null) {
      return static d => _ => d.ImplementationInstance!;
    }

    if (descriptor.ImplementationFactory is not null) {
      return static d => d.ImplementationFactory!;
    }

    if (descriptor.ImplementationType is not null) {
      return static d => injector => InjectorTypeActivator.CreateInstance(d.ImplementationType!, injector);
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