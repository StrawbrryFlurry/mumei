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

    if (!_providers.TryGet(token, out var descriptor)) {
      provider = default!;
      return false;
    }

    if (TryMakeDynamicBinding(descriptor!, out binding)) {
      provider = binding.GetInstance(_injector);
      return true;
    }

    provider = default!;
    return false;
  }

  private bool TryMakeDynamicBinding(ProviderDescriptor descriptor, out Binding binding) {
    var token = descriptor.Token;

    var factory = MakeDescriptorFactory(descriptor);

    if (descriptor.Lifetime is InjectorLifetime.Scoped) {
      binding = new DynamicScopedBinding(factory(descriptor), _injector);
      _bindings.Add(token, binding);
      return true;
    }

    if (descriptor.Lifetime is InjectorLifetime.Singleton) {
      binding = new DynamicSingletonBinding(factory(descriptor)(_injector));
      _bindings.Add(token, binding);
      return true;
    }

    if (descriptor.Lifetime is InjectorLifetime.Transient) {
      binding = new DynamicTransientBinding(factory(descriptor), _injector);
      _bindings.Add(token, binding);
      return true;
    }

    binding = default!;
    return false;
  }

  private Func<ProviderDescriptor, Func<IInjector, object>> MakeDescriptorFactory(ProviderDescriptor descriptor) {
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
}