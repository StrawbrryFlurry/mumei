namespace Mumei.DependencyInjection.Attributes;

public class Singleton : Attribute { }

public sealed class SingletonAttribute<TProvider>
  : DependencyRegistrationAttribute<TProvider> { }