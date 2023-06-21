namespace Mumei.DependencyInjection.Core;

public sealed class NullInjector : IInjector {
  public static readonly NullInjector Instance = new();

  public IInjector Parent => throw new InvalidOperationException("Cannot get parent of NullInjector");

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    throw new NullInjectorException(typeof(TProvider));
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    throw new NullInjectorException(token);
  }

  public static NullInjectorException Exception(object provider) {
    throw new NullInjectorException(provider);
  }

  public sealed class NullInjectorException : Exception {
    public NullInjectorException(object type) : base(
      $"Provider of type {type.GetType().FullName} not found in scope: {{Scope}}") { }
  }
}