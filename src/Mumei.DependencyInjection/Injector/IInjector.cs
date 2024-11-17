using Mumei.DependencyInjection.Injector.Behavior;

namespace Mumei.DependencyInjection.Injector;

/// <summary>
/// TODO: For super speedy provider lookup:
/// All injectors(modules) contain a Map{long, long} where the
/// key is a unique type id (constructed from the type's full name at compile time)
/// and the value is the index of the module's provider in the global provider binding array.
/// All module's bindings are stored in that global array and each of them gets a unique index.
/// The lookup could be vectorized if we know all type ids at compile time and therefore not use a map?
/// <code>
/// class WeatherModule {
///   public WeatherServiceBinding __WeatherServiceBinding { get; init; }
///
///   public T? Get{T}(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
///     var typeId = typeof(T).GetTypeId(); // this needs to be fast!
///     if (_moduleProviders.TryGetValue(typeId, out var index)) {
///       return InjectorView.Bindings[index].Get(this, scope, flags);
///     }
///   }
/// 
///   public static long[] _moduleProviderBloomFilter = { ... }
/// 
///   public static readonly FastLongKeyedMap{long, long} _moduleProviders = {
///     { WEATHER_SERVICE_TYPE_ID, 2 }
///   }
/// }
///
/// class InjectorView {
///   static readonly Binding[] Bindings = {
///     RootInjector.Injector,
///     WeatherModule.Injector,
///     WeatherModule.__WeatherServiceBinding
///   };
/// }
/// </code>
/// Other modules importing WeatherModule can now just check the bloom filters up the injector chain
/// until they find a match, then point to the global binding array.
/// </summary>
public interface IInjector {
  /// <summary>
  ///   The injector that was used to create this injector.
  ///   Usually, if this injector is a module, the parent will
  ///   be the module who has imported this module, likewise, if
  ///   this injector is a component, the parent will be the module
  ///   that has imported the component.
  /// </summary>
  public IInjector Parent { get; }

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None);

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None);
}