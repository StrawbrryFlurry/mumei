namespace Mumei.Core.Attributes;

public class Singleton : Attribute { }

public class SingletonAttribute<TProvider, TImplementation>
  : DependencyRegistrationAttribute<TProvider, TImplementation>
  where TImplementation : TProvider { }

public class SingletonAttribute<TProvider>
  : DependencyRegistrationAttribute<TProvider> { }

/// <summary>
///   Adds a singleton provider to the module without making
///   it publicly available to other modules that import its parent.
/// </summary>
/// <typeparam name="TProvider"></typeparam>
/// <typeparam name="TImplementation"></typeparam>
public class InternalSingletonAttribute<TProvider, TImplementation>
  : SingletonAttribute<TProvider, TImplementation> where TImplementation : TProvider { }

public class InternalSingletonAttribute<TProvider> : SingletonAttribute<TProvider> { }

public class InternalAttribute : Attribute { }