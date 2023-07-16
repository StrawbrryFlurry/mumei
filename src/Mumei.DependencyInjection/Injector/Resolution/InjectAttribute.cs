namespace Mumei.DependencyInjection.Injector.Resolution;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class InjectAttribute : Attribute {
  public InjectAttribute() { }

  public InjectAttribute(string providerName) {
    ProviderName = providerName;
  }

  public string? ProviderName { get; }
}