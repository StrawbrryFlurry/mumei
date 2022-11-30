namespace Mumei.Attributes;

public class Singleton : Attribute { }

public class SingletonAttribute<TProvider>
  : DependencyRegistrationAttribute<TProvider> { }