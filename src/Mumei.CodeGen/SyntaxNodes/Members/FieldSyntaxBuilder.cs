using Mumei.CodeGen.SyntaxBuilders;

namespace Mumei.CodeGen.SyntaxNodes;

public class FieldSyntaxBuilder : MemberSyntaxBuilder<FieldSyntax> {
  protected object? Initializer;

  public FieldSyntaxBuilder(TypeSyntax parent, string name, Type type) : base(name, type, parent) {
  }

  public override FieldSyntax Build() {
    var field = MakeSyntaxInstance();
    field.Initializer = Initializer;

    return field;
  }

  public FieldSyntaxBuilder SetInitialValue(object? value) {
    Initializer = value;
    return this;
  }
}