using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class AccessorListSyntax : Syntax {
  public AccessorListSyntax(Syntax? parent = null) : base(parent) {
    Accessors = (null, null)!;
  }

  public AccessorListSyntax(
    AccessorSyntax get,
    AccessorSyntax? set = null,
    Syntax? parent = null) : base(parent) {
    DefineGetter(get);
    DefineSetter(set);
  }

  public (
    AccessorSyntax Getter,
    AccessorSyntax? Setter
    ) Accessors { get; private set; }

  public void DefineGetter(AccessorSyntax getter) {
    AssertValidGetter(getter);
    getter.SetParent(this);
    Accessors = (getter, Accessors.Setter);
  }

  public void DefineSetter(AccessorSyntax? setter) {
    AssertValidSetter(setter);
    setter?.SetParent(this);
    Accessors = (Accessors.Getter, setter);
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    AssertHasGetter();

    writer.WriteLine("{");
    writer.Indent();

    Accessors.Getter.WriteAsSyntax(writer);

    if (Accessors.Setter is not null) {
      writer.WriteLine();
      Accessors.Setter.WriteAsSyntax(writer);
    }

    writer.WriteLine();
    writer.UnIndent();
    writer.Write("}");
  }

  private void AssertHasGetter() {
    if (Accessors.Getter is null) {
      throw new InvalidOperationException("Invalid accessor list. No getter is defined");
    }
  }

  private void AssertValidGetter(AccessorSyntax accessor) {
    if (accessor.AccessorType is not AccessorType.Get) {
      throw new ArgumentException("Invalid accessor. Getter must be be of type get");
    }
  }

  private void AssertValidSetter(AccessorSyntax? accessor) {
    if (accessor is { AccessorType: not (AccessorType.Init or AccessorType.Set) }) {
      throw new ArgumentException("Invalid accessor. Setter must be either init or set");
    }
  }

  public override Syntax Clone() {
    var clone = new AccessorListSyntax(Accessors.Getter, Accessors.Setter);
    return clone;
  }
}