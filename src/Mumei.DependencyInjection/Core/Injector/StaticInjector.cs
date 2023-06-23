namespace Mumei.DependencyInjection.Core;

public sealed class StaticInjector : IInjector {
  public IInjector Parent { get; }

  private readonly List<(object Token, Binding Binding)> _bindings = new();

  private StaticInjector(IInjector parent) {
    Parent = parent;
  }

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return _bindings.FirstOrDefault(x => x.Token == token).Binding.GetInstance(this);
  }

  public static StaticInjector Create(
    IInjector parent,
    Action<StaticInjectorProviderCollection> configureProviderCollection
  ) {
    var collection = new StaticInjectorProviderCollection();
    configureProviderCollection(collection);

    var injector = new StaticInjector(parent);
    injector._bindings.AddRange(collection.Bindings);

    return injector;
  }

  public sealed class StaticInjectorProviderCollection {
    internal List<(object Token, Binding Instance)> Bindings { get; } = new();

    public StaticInjectorProviderCollection Add<TToken, TInstance>(TInstance instance) where TInstance : TToken {
      return AddToCollection(typeof(TToken), typeof(TToken), instance!);
    }

    public StaticInjectorProviderCollection Add<TToken>(TToken instance) {
      return AddToCollection(typeof(TToken), typeof(TToken), instance!);
    }

    public StaticInjectorProviderCollection Add(object token, object instance) {
      if (token is not Type bindingType) {
        bindingType = instance.GetType();
      }

      return AddToCollection(bindingType, token, instance);
    }

    private StaticInjectorProviderCollection AddToCollection(Type bindingType, object token, object instance) {
      Bindings.Add((token, DynamicSingletonBinding.CreateDynamic(bindingType, instance)));
      return this;
    }
  }
}