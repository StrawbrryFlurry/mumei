namespace Mumei.DependencyInjection.Attributes;

public class TransientAttribute<TProvider, TImplementation>
  : DependencyRegistrationAttribute<TProvider, TImplementation>
  where TImplementation : TProvider { }

public class TransientAttribute<TProvider>
  : DependencyRegistrationAttribute<TProvider> { }

/// <summary>
///   Adds a transient provider to the module without making
///   it publicly available to other modules that import its parent.
/// </summary>
/// <typeparam name="TProvider"></typeparam>
/// <typeparam name="TImplementation"></typeparam>
public class InternalTransientAttribute<TProvider, TImplementation> :
  TransientAttribute<TProvider, TImplementation>
  where TImplementation : TProvider { }

public class InternalTransientAttribute<TProvider> : TransientAttribute<TProvider> { }