namespace Mumei.DependencyInjection.Providers.Registration;

public sealed class SingletonAttribute : Attribute { }

public sealed class SingletonAttribute<TProvider>
  : DependencyRegistrationAttribute<TProvider> { }