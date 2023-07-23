using System.Reflection;
using Mumei.Common.Utilities;

namespace Mumei.Common.Reflection;

internal sealed class ReflectionCustomAttributeData : CustomAttributeData {
  private static readonly FieldInfo CustomAttributeDataAttributeTypeField = typeof(CustomAttributeData)
    .GetProperty(nameof(AttributeType))!
    .GetBackingField();

  public override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }
  public override IList<CustomAttributeNamedArgument> NamedArguments { get; }
  public override ConstructorInfo Constructor { get; }

  public ReflectionCustomAttributeData(
    ConstructorInfo attributeCtor,
    IList<CustomAttributeTypedArgument> constructorArguments,
    IList<CustomAttributeNamedArgument> namedArguments
  ) {
    Constructor = attributeCtor;
    ConstructorArguments = constructorArguments;
    NamedArguments = namedArguments;
  }
}