﻿namespace Mumei.Core; 

public sealed class DecoratedInjector : IInjector {
  public IInjector DecoratorInjector { get; }
  public IInjector Parent { get; }

  public DecoratedInjector(IInjector injectorToDecorate, IInjector decoratorInjector) {
    Parent = injectorToDecorate;
    DecoratorInjector = decoratorInjector;
  }
  
  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    return DecoratorInjector.Get<TProvider>(flags & InjectFlags.Optional) ?? Parent.Get<TProvider>(flags);
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    return DecoratorInjector.Get(token, flags & InjectFlags.Optional) ?? Parent.Get(token, flags);
  }
}