namespace Mumei.DependencyInjection.Core;

public sealed class NullInjector : IInjector {
  public static readonly NullInjector Instance = new();

  public IInjector Parent => throw new InvalidOperationException("Cannot get parent of NullInjector");

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    throw new NullInjectorException(typeof(TProvider), scope);
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    throw new NullInjectorException(token, scope);
  }

  public static NullInjectorException Exception(object token, IInjector? scope = null) {
    throw new NullInjectorException(token, scope);
  }

  public sealed class NullInjectorException : Exception {
    public NullInjectorException(object token, IInjector? injector) : base(
      $"Provider of type {token} not found in injector: {injector?.GetType()?.FullName ?? "Undefined global scope"}"
    ) { }
  }
}