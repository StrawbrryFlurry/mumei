namespace Mumei.DependencyInjection.Core;

public interface IComponentFactory : IComponentFactory<IComponent> { }

public interface IComponentFactory<TComponent> where TComponent : IComponent { }