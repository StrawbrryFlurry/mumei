namespace Mumei.DependencyInjection.Providers.Registration;

/// <summary>
/// Configures the specified provider through the attributed method.
/// The provider type can either be specified by the return type of the method
/// or by using the <see cref="ProvideAttribute"/>. Configurations will be applied in the order
/// of their declaration.
/// </summary>
/// <typeparam name="TConfigureProvider">The provider for whose resolution this configuration is applied</typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ConfigureForAttribute<TConfigureProvider> : Attribute { }

/// <summary>
/// Configures the specified provider through the attributed method.
/// The provider type can either be specified by the return type of the method
/// or by using the <see cref="ProvideAttribute"/>. The configuration will
/// be applied to all providers of the specified type. Configurations will be applied in the order
/// of their declaration.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ConfigureAttribute : Attribute { }