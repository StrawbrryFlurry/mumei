namespace Mumei.DependencyInjection.Core;

public sealed class DynamicInjector : List<(object Token, Binding Binding)>, IInjector {
  public DynamicInjector(IInjector parent) {
    Parent = parent;
  }

  public IInjector Parent { get; }

  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), flags);
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    return this.FirstOrDefault(x => x.Token == token).Binding.GetInstance(this);
  }

  public void Add(object token, object instance) {
    var binding = new SingletonBindingFactoryImpl(instance);
    Add((token, binding));
  }

  public void Add(Type type, Binding binding) {
    Add((type, binding));
  }

  public void Add(object token, Binding binding) {
    Add((token, binding));
  }
}