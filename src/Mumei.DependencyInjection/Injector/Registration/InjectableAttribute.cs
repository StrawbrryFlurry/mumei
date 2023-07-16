namespace Mumei.DependencyInjection.Injector.Registration;

/// <summary>
///  Allows users to dynamically add a provider to
///  "any" injector in the hierarchy. Usually, this
///  will be any of the high level auto-generated
/// injectors such as <see cref="Registration.ProvidedIn.Root" /> or
/// <see cref="Registration.ProvidedIn.Environment"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class InjectableAttribute : Attribute {
  public readonly ProvidedIn ProvidedIn;

  public InjectableAttribute(ProvidedIn providedIn = ProvidedIn.Root) {
    ProvidedIn = providedIn;
  }
}

/// Allows users to dynamically add a provider
/// to the declared module.
/// <typeparam name="TModule">The module to add the provider to</typeparam>
[AttributeUsage(AttributeTargets.Class)]
public sealed class InjectableAttribute<TModule> : Attribute {
  public readonly ProvidedIn ProvidedIn;

  public InjectableAttribute(ProvidedIn providedIn = ProvidedIn.Root) {
    ProvidedIn = providedIn;
  }

  public Type ModuleType => typeof(TModule);
}