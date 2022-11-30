namespace Mumei.Core; 

public interface IComponentFactoryResolver {
  public IComponentFactory<TComponent> Resolve<TComponent>() where TComponent : IComponent;
  public IComponentFactory Resolve(Type componentType);
}