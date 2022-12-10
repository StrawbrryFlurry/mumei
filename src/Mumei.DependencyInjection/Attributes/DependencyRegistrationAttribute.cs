namespace Mumei.DependencyInjection.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = true)]
public abstract class DependencyRegistrationAttribute<TProvider, TImplementation> : Attribute
  where TImplementation : TProvider { }

[AttributeUsage(
  AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property,
  AllowMultiple = true)]
public abstract class DependencyRegistrationAttribute<TProvider> : Attribute { }