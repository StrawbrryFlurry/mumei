namespace Mumei.DependencyInjection.Providers.Registration;

public enum ForwardRefSource {
  /// <summary>
  /// Declares that the provider can be resolved through the parent module.
  /// Because the parent module is always able to resolve all providers in it's children,
  /// this can also be used for declaring providers that are part of sibling component implementations in different
  /// modules.
  /// </summary>
  Parent,

  /// <summary>
  /// Declares that the provider is available through the parent injector, same as <see cref="Parent"/>,
  /// but disables static analysis for the provider when building the dependency graph.
  /// </summary>
  Dynamic,

  Default = Parent
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ForwardRefAttribute : Attribute {
  public ForwardRefSource Source { get; }

  public ForwardRefAttribute(ForwardRefSource source = ForwardRefSource.Default) {
    Source = source;
  }
}