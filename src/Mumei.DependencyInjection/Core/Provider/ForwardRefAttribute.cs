namespace Mumei.DependencyInjection.Core;

public enum ForwardRefSource {
  /// <summary>
  /// Requires the provider to be declared in a parent component or module.
  /// </summary>
  Parent,

  /// <summary>
  /// Requires the provider to be declared in a sibling of a subcomponent.
  /// </summary>
  Sibling
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ForwardRefAttribute : Attribute {
  public ForwardRefSource Source { get; }

  public ForwardRefAttribute() { }

  public ForwardRefAttribute(ForwardRefSource source) {
    Source = source;
  }
}