namespace Mumei.DependencyInjection.Providers.Registration;

/// <summary>
/// Provides an existing provider under a different type.
/// <code>
/// interface ISomeModule {
///  [UseExisting{ISomeProvider}]
///  public ISomeOtherProvider SomeOtherProvider { get; }
/// }
/// </code>
/// When using this attribute on a method, users may configure the
/// existing provider through the provider's value passed as a parameter.
/// </summary>
/// <typeparam name="TExistingProvider"></typeparam>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class UseExistingAttribute<TExistingProvider> : Attribute { }