namespace Mumei.DependencyInjection.Providers.Registration;

public class Singleton : Attribute { }

public sealed class SingletonAttribute<TProvider>
  : DependencyRegistrationAttribute<TProvider> { }