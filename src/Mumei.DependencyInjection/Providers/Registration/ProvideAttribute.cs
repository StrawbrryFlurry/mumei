namespace Mumei.DependencyInjection.Providers.Registration;

/// <summary>
/// Configures the provider to be retrievable by a custom provider token
/// that can be specified using the <see cref="ProvideAttribute"/> constructor.
/// <remarks>
/// The provider token needs to be a compile time constant and must also be provided 
/// </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class ProvideAttribute : Attribute {
  public string? Token { get; }

  public ProvideAttribute() { }

  public ProvideAttribute(string token) {
    Token = token;
  }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class ProvideAttribute<TProviderToken> : Attribute { }

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
public sealed class ProvideSingletonAttribute : Attribute { }

/// <inheritdoc cref="ProvideSingletonAttribute"/>
public sealed class ProvideTransientAttribute : Attribute { }

/// <inheritdoc cref="ProvideSingletonAttribute"/>
public sealed class ProvideScopedAttribute : Attribute { }