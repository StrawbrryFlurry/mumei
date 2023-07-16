namespace Mumei.DependencyInjection.Injector.Behavior;

[AttributeUsage(AttributeTargets.Parameter)]
public abstract class InjectBehaviorAttribute : Attribute {
  public InjectFlags BehaviorFlags { get; }

  protected InjectBehaviorAttribute(InjectFlags behaviorFlags) {
    BehaviorFlags = behaviorFlags;
  }
}