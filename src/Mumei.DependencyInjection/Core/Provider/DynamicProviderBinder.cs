namespace Mumei.DependencyInjection.Core;

public abstract class DynamicProviderBinder {
  private readonly IInjector _injector;
  private readonly Dictionary<object, Binding> _bindings = new();
  private readonly ProviderCollection _providers = new();

  public DynamicProviderBinder(
    IInjector injector,
    Action<ProviderCollection> configureBinder
  ) {
    _injector = injector;
    configureBinder(_providers);
  }

  public bool TryGet(object token, out object provider) {
    if (_bindings.TryGetValue(token, out var binding)) {
      provider = binding.GetInstance(_injector);
      return true;
    }

    ProviderDescriptor? descriptor = null;
    if (IsProviderBinding(token, out var bindingType)) {
      if (_bindings.TryGetValue(bindingType, out binding)) {
        provider = binding;
        return true;
      }

      if (!_providers.TryGet(bindingType, out descriptor)) {
        provider = default!;
        return false;
      }

      provider = MakeDynamicBinding(descriptor!);
      return true;
    }

    if (!_providers.TryGet(token, out descriptor)) {
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
      binding = new DynamicScopedBinding(factory(descriptor), _injector);
      _bindings.Add(token, binding);
      return binding;
    }

    if (descriptor.Lifetime is InjectorLifetime.Transient) {
      binding = DynamicTransientBinding.CreateDynamic(bindingType, factory(descriptor), _injector);
      _bindings.Add(token, binding);
      return binding;
    }

    binding = new DynamicSingletonBinding(factory(descriptor)(_injector));
    _bindings.Add(token, binding);
    return binding;
  }

  private static Func<ProviderDescriptor, Func<IInjector, object>>
    MakeDescriptorFactory(ProviderDescriptor descriptor) {
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

  private static bool IsProviderBinding(object token, out Type bindingType) {
    if (token is not Type type) {
      bindingType = null!;
      return false;
    }

    var isBinding = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Binding<>);
    if (!isBinding) {
      bindingType = null!;
      return false;
    }

    bindingType = type.GetGenericArguments()[0];
    return true;
  }
}