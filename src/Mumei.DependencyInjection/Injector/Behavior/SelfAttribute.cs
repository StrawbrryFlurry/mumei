namespace Mumei.DependencyInjection.Injector.Behavior;

/// <inheritdoc cref="InjectFlags.Self"/>
public sealed class SelfAttribute : InjectBehaviorAttribute {
  public SelfAttribute() : base(InjectFlags.Self) { }
}