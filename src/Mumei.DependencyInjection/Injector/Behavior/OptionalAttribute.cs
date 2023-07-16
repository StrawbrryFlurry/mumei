namespace Mumei.DependencyInjection.Injector.Behavior;

/// <inheritdoc cref="InjectFlags.Optional"/>
public class OptionalAttribute : InjectBehaviorAttribute {
  public OptionalAttribute() : base(InjectFlags.Optional) { }
}