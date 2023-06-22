namespace Mumei.DependencyInjection.Core;

public sealed class LateBoundBinding<TProvider> : Binding<TProvider> {
  private readonly IInjector _injector;
  private readonly Func<IInjector, Binding<TProvider>?> _accessor;

  private Binding<TProvider>? _bindingInstance;
  private Binding<TProvider> BindingInstance => _bindingInstance ??= LoadBindingInstance();

  public LateBoundBinding(IInjector injector, Func<IInjector, Binding<TProvider>?> accessor) {
    _injector = injector;
    _accessor = accessor;
  }

  public override TProvider Get(IInjector? scope = null) {
    return BindingInstance.Get(scope);
  }

  protected internal override TProvider Create(IInjector? scope = null) {
    return BindingInstance.Create(scope);
  }

  private Binding<TProvider> LoadBindingInstance() {
    var instance = _accessor(_injector);
    if (instance is not null) {
      return instance;
    }

    throw new InvalidOperationException(
      $"Cannot access late bound binding of type {typeof(TProvider).FullName} in {_injector.GetType().FullName} before the dependency tree has been realized."
    );
  }
}