namespace Mumei.Core.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class DependencyRegistrationAttribute<TProvider, TImplementation> : Attribute
  where TImplementation : TProvider { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class DependencyRegistrationAttribute<TProvider> : Attribute { }