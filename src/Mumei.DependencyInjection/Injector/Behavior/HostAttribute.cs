namespace Mumei.DependencyInjection.Injector.Behavior;

/// <inheritdoc cref="InjectFlags.Host"/>
public sealed class HostAttribute : InjectBehaviorAttribute {
  public HostAttribute() : base(InjectFlags.Host) { }
}