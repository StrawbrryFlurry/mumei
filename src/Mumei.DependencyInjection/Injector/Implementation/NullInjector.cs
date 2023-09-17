using System.Diagnostics.CodeAnalysis;
using Mumei.DependencyInjection.Injector.Behavior;

namespace Mumei.DependencyInjection.Injector.Implementation;

public sealed class NullInjector : IInjector {
  public static readonly NullInjector Instance = new();

  public IInjector Parent => throw new InvalidOperationException("Cannot get parent of NullInjector");

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags) ?? default!;
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    if ((flags & InjectFlags.Optional) != 0) {
      return null!;
    }

    throw new NullInjectorException(token, scope);
  }

  [DoesNotReturn]
  public static void Throw(object token, IInjector? scope = null) {
    throw new NullInjectorException(token, scope);
  }

  public sealed class NullInjectorException : Exception {
    public NullInjectorException(object token, IInjector? injector) : base(
      $"Provider of type {token} not found in injector: {injector?.GetType()?.FullName ?? "Undefined global scope"}"
    ) { }
  }
}