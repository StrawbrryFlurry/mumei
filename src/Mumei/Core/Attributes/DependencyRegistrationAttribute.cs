namespace Mumei.Core.Attributes;

public abstract class DependencyRegistrationAttribute<TProvider, TImplementation> : Attribute { }

public abstract class DependencyRegistrationAttribute<TProvider> : Attribute { }