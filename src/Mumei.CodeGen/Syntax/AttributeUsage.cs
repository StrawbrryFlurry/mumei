namespace Mumei.CodeGen.Syntax;

public struct AttributeUsage {
  public Type Type;
  public object[] Arguments = Array.Empty<object>();
  public Dictionary<NamedAttributeParameter, object> NamedArguments = new();

  public AttributeUsage(Type type) {
    Type = type;
  }

  public static AttributeUsage Create<TAttribute>(
  ) where TAttribute : Attribute {
    return new AttributeUsage(typeof(TAttribute));
  }

  public static AttributeUsage Create<TAttribute>(
    params object[] arguments
  ) where TAttribute : Attribute {
    return new AttributeUsage(typeof(TAttribute)) {
      Arguments = arguments
    };
  }

  public static AttributeUsage Create<TAttribute>(
    Dictionary<NamedAttributeParameter, object> namedArguments,
    params object[] arguments
  ) where TAttribute : Attribute {
    return new AttributeUsage(typeof(TAttribute)) {
      Arguments = arguments,
      NamedArguments = namedArguments
    };
  }
}

public struct NamedAttributeParameter {
  public string Name;

  /// <summary>
  ///   When true uses `[Attribute(Name = "Name")]` instead of `[Attribute(name: "Name")]`
  /// </summary>
  public bool IsField;

  public static implicit operator NamedAttributeParameter(string name) {
    return new NamedAttributeParameter {
      Name = name
    };
  }
}