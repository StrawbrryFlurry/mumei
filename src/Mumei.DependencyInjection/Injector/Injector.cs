using Mumei.DependencyInjection.Injector.Behavior;
using Mumei.DependencyInjection.Injector.Implementation;

namespace Mumei.DependencyInjection.Injector;

public static class Injector {
  internal static AsyncLocal<IInjector> CurrentInjector { get; } = new();

  /// <summary>
  /// Resolves the provider of type <typeparamref name="T"/> from the
  /// current injector context. Usually, the current injector is the module
  /// injector being used to resolve either the current class or a provider
  /// referencing this class in any way. If <see cref="Inject{T}"/> is called
  /// outside the init context of a class (e.g. field initialization or the constructor),
  /// the platform, application or scope injector is used, depending on the context where it is called.
  /// <example>
  /// <code>
  /// using static Mumei.DependencyInjection.Injector.Injector;
  /// public sealed class SomeProvider {
  ///   private readonly IOtherProvider _provider = Inject{IOtherProvider}();
  /// }
  ///
  /// public sealed class SomeController {
  /// public IActionResult Get() {
  ///   var provider = Inject{SomeProvider}(); // Will return the provider instance from the current scope context.
  ///   return Ok();
  /// }
  /// }
  /// </code>
  /// </example>
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public static T Inject<T>(InjectFlags flags = InjectFlags.None) {
    var injector = CurrentInjector.Value;
    if (injector is null) {
      throw new InvalidOperationException(
        $"Cannot use {nameof(Injector)}.{nameof(Inject)} outside of an injector context."
      );
    }

    return injector.Get<T>(flags: flags);
  }

  public static IInjector Create(
    IInjector? parent,
    Action<StaticInjector.StaticInjectorProviderCollection> providers
  ) {
    return StaticInjector.Create(parent, providers);
  }
}