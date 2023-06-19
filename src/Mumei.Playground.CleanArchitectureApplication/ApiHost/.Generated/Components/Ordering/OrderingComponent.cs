using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λOrderingComponent : IOrderingComponentComposite, IComponent {
  public IInjector Parent { get; }
  public IInjector Scope { get; }

  internal readonly Binding<IOrderRepository> OrderRepositoryBinding;

  public λOrderingComponent(IInjector scope, IInjector parent) {
    Scope = scope;
    Parent = parent;
    OrderRepositoryBinding = new OrderRepositoryλBinding();
  }

  public IOrderRepository OrderRepository => OrderRepositoryBinding.Get();


  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    var token = typeof(TProvider);
    var result = token switch {
      _ when ReferenceEquals(token, typeof(IOrderRepository)) => OrderRepositoryBinding.Get(Scope),
      _ => Parent.Get(token)
    };

    return (TProvider)result;
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    return token switch {
      _ when ReferenceEquals(token, typeof(IOrderRepository)) => OrderRepositoryBinding.Get(Scope),
      _ => Parent.Get(token)
    };
  }


  public IInjector CreateScope() {
    return new OrderingComponentλScope(Parent, this);
  }

  public IInjector CreateScope(IInjector context) {
    var decoratedParent = new DecoratedInjector(context, this);
    return new OrderingComponentλScope(decoratedParent, this);
  }

  private sealed class OrderingComponentλScope : IInjector {
    public IInjector Parent { get; }

    private readonly λOrderingComponent _component;

    public OrderingComponentλScope(IInjector parent, λOrderingComponent component) {
      _component = component;
      Parent = parent;
    }

    public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
      var token = typeof(TProvider);
      var result = token switch {
        _ when ReferenceEquals(token, typeof(IOrderRepository)) => _component.OrderRepositoryBinding.Get(this),
        _ => Parent.Get(token)
      };

      return (TProvider)result;
    }

    public object Get(object token, InjectFlags flags = InjectFlags.None) {
      return token switch {
        _ when ReferenceEquals(token, typeof(IOrderRepository)) => _component.OrderRepositoryBinding.Get(this),
        _ => Parent.Get(token)
      };
    }
  }
}