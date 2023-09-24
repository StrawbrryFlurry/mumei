namespace Mumei.DependencyInjection.Providers.Registration;

[AttributeUsage(AttributeTargets.Method)]
public sealed class DecorateExistingAttribute<TExistingProvider> : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public sealed class DecorateExistingAttribute<TExistingProvider, TDecorated> : Attribute
  where TDecorated : TExistingProvider { }