namespace Mumei.DependencyInjection.Core;

/// <summary>
///   Marks a method as a dynamic provider binding to the module.
///   The return type of the method will be used as the provider token,
///   while the return value will be used as it's instance.
///   Note: Dynamic bindings may not circularly reference other dynamic bindings.
///   <code>
/// [Singleton]
/// [DynamicBinding]
/// public IWeatherService SingletonWeatherService() {
///  return new WeatherService(CommonModule.HttpClient);
/// }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class Provides : Attribute {
  public Provides() { }

  public Provides(string token) {
    Token = token;
  }

  public string? Token { get; }
}