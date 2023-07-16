namespace Mumei.DependencyInjection.Injector.Behavior;

/// <inheritdoc cref="InjectFlags.SkipSelf"/>
public sealed class SkipSelfAttribute : InjectBehaviorAttribute {
  public SkipSelfAttribute() : base(InjectFlags.SkipSelf) { }
}