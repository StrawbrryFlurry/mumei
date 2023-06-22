﻿using System.Diagnostics;

namespace Mumei.DependencyInjection.Core;

public abstract class Binding {
  public abstract object GetInstance(IInjector scope = null!);
}

public abstract class Binding<TProvider> : Binding {
  public abstract TProvider Get(IInjector? scope = null);
  protected internal abstract TProvider Create(IInjector? scope = null);

  [DebuggerHidden]
  [StackTraceHidden]
  public override object GetInstance(IInjector? scope = null) {
    return Get(scope)!;
  }
}