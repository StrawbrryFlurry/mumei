namespace Mumei.DependencyInjection.Core;

public sealed class DecoratedInjector : IInjector {
  public DecoratedInjector(IInjector injectorToDecorate, IInjector decoratorInjector) {
    Parent = injectorToDecorate;
    DecoratorInjector = decoratorInjector;
  }

  public IInjector DecoratorInjector { get; }
  public IInjector Parent { get; }

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return DecoratorInjector.Get<TProvider>(scope, flags & InjectFlags.Optional) ?? Parent.Get<TProvider>(scope, flags);
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    return DecoratorInjector.Get(token, scope, flags & InjectFlags.Optional) ?? Parent.Get(token, scope, flags);
  }
}