namespace Mumei.DependencyInjection.Providers.Registration;

public sealed class TransientAttribute : Attribute { }

public sealed class TransientAttribute<TProvider> : Attribute { }

/// <summary>
///   Adds a transient provider to the module without making
///   it publicly available to other modules that import its parent.
/// </summary>
/// <typeparam name="TProvider"></typeparam>
/// <typeparam name="TImplementation"></typeparam>
public class InternalTransientAttribute<TProvider, TImplementation> : Attribute { }

public class InternalTransientAttribute<TProvider> : Attribute { }