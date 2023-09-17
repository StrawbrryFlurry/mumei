namespace Mumei.DependencyInjection.Providers.Registration;

/// <summary>
///   Marks a method as a factory provider binding to the module.
///   The return type of the method will be used as the provider token,
///   while the return value will be used as it's instance.
///   Note: Factory providers may not circularly reference other factory providers.
///   <code>
/// [ProvideSingleton]
/// public IWeatherService SingletonWeatherService() {
///  return new WeatherService(CommonModule.HttpClient);
/// }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public abstract class ProvideAttribute : Attribute {
  public ProvideAttribute() { }

  public ProvideAttribute(string token) {
    Token = token;
  }

  public string? Token { get; }
}

/// <inheritdoc cref="ProvideAttribute"/>
public sealed class ProivdeSingletonAttribute : ProvideAttribute { }

/// <inheritdoc cref="ProvideAttribute"/>
public sealed class ProvideTransientAttribute : ProvideAttribute { }

/// <inheritdoc cref="ProvideAttribute"/>
public sealed class ProvideScopedAttribute : ProvideAttribute { }