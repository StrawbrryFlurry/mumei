namespace Mumei.DependencyInjection.Module.Registration;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public sealed class ContributesAttribute<TComponent> : Attribute where TComponent : IComponent { }