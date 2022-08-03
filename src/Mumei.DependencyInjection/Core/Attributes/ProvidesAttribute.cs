namespace Mumei.Core.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ProvidesAttribute<TProvides> : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class ProvidesAttribute : Attribute {
  public readonly Type ProvidesType;

  public ProvidesAttribute(Type type) {
    ProvidesType = type;
  }
}