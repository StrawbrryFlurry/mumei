using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Members;

public class PropertySyntaxTest {
  public void AddAutoGetter_() {
    var sut = MakeProperty();
  }

  public void AddAutoSetter_() {
    var sut = MakeProperty();
  }

  public void AddGetter_() {
    var sut = MakeProperty();
  }

  public void AddSetter_() {
    var sut = MakeProperty();
  }

  public void AddGetterWithBackingField_() {
    var sut = MakeProperty();
  }

  public void AddSetterWithBackingField() {
    var sut = MakeProperty();
  }

  private PropertySyntax MakeProperty() {
    var property = new PropertySyntax("property", null!);
    return property;
  }
}