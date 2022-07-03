using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxBuilders;

public abstract class MemberSyntaxBuilder : MemberSyntaxWriter {
  protected readonly Dictionary<Type, object[]> Attributes = new();

  public void AddAttribute(Type attributeType, params object[] arguments) {
    Attributes.Add(attributeType, arguments);
  }

  public void AddAttribute<TAttribute>(params object[] arguments) where TAttribute : Attribute {
    AddAttribute(typeof(TAttribute), arguments);
  }

  protected string GetAttributeString() {
    var writer = new AttributeSyntaxWriter(new WriterTypeContext());

    foreach (var (attributeType, arguments) in Attributes) {
      writer.WriteAttribute(attributeType, arguments);
    }

    return writer.ToString();
  }
}