namespace Mumei.DependencyInjection.Providers.Registration;

/// <summary>
/// TODO:
/// When using transient providers, users may allocate lots of memory for that provider, every time it is resolved.
/// Because transient registration never caches the resolved instance, consumers may not benefit of the provider's heap
/// allocation, in the default case of a class provider.
///
/// Using this provider type, users can specify a struct that is only allocated on the heap,
/// and passed as a `in` ref to the consumer.
///
/// <code>
/// public struct WeatherService : IWeatherService {
///   public WeatherService(string apiKey) { ... }
/// 
///   public Weather GetWeather(string location) { ... }
/// }
///
/// 
/// [Module]
/// public interface WeatherModule {
///   [TransientByRef&lt;WeatherService&gt;]
///   IWeatherService GetWeatherService(string apiKey);
/// }
///
/// 
/// ref var service = ref module.GetByRef&lt;IWeatherService&gt;();
/// </code>
///
/// <remarks>The type may only be registered at compile time and does not allow for dynamic registration of a by ref provider</remarks>
/// </summary>
/// <typeparam name="TProvideByRef">The implementation struct to be provided by ref</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public sealed class TransientByRefAttribute<TProvideByRef> : Attribute where TProvideByRef : struct { }