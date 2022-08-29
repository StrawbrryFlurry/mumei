namespace Mumei.Core.Attributes;

public class ScopedAttribute<TProvider, TImplementation> :
  DependencyRegistrationAttribute<TProvider, TImplementation> where TImplementation : TProvider { }

public class ScopedAttribute<TProvider> : DependencyRegistrationAttribute<TProvider> { }