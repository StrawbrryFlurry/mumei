using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class ReflectionCustomAttributeData : CustomAttributeData {
  public ReflectionCustomAttributeData(
    Type attributeType,
    IList<CustomAttributeTypedArgument> constructorArguments,
    IList<CustomAttributeNamedArgument> namedArguments
  ) {
    AttributeType = attributeType;
    ConstructorArguments = constructorArguments;
    NamedArguments = namedArguments;
  }

  public override Type AttributeType { get; }

  public override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }
  public override IList<CustomAttributeNamedArgument> NamedArguments { get; }
}
