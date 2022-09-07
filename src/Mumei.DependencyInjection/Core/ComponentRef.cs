using Mumei.Core.Provider;

namespace Mumei.Core;

public abstract class ComponentRef<TComponent> {
  public abstract Type Type { get; }
  public abstract Binding<TComponent> Instance { get; }
}