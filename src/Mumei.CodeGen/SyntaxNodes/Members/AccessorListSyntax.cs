using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class AccessorListSyntax : Syntax {
  public AccessorListSyntax(Syntax? parent = null) : base(parent) { }

  public AccessorListSyntax(
    AccessorSyntax? get,
    AccessorSyntax? set = null,
    Syntax? parent = null) : base(parent) {
    DefineGetter(get);
    DefineSetter(set);
  }

  public AccessorSyntax? Getter { get; private set; }
  public AccessorSyntax? Setter { get; private set; }

  public void DefineGetter(AccessorSyntax? getter) {
    AssertValidGetter(getter);
    getter?.SetParent(this);
    Getter = getter;
  }

  public void DefineSetter(AccessorSyntax? setter) {
    AssertValidSetter(setter);
    setter?.SetParent(this);
    Setter = setter;
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    AssertHasGetter();

    writer.WriteLine("{");
    writer.Indent();

    Getter!.WriteAsSyntax(writer);

    if (Setter is not null) {
      writer.WriteLine();
      Setter.WriteAsSyntax(writer);
    }

    writer.WriteLine();
    writer.UnIndent();
    writer.Write("}");
  }

  private void AssertHasGetter() {
    if (Getter is null) {
      throw new InvalidOperationException("Invalid accessor list. No getter is defined");
    }
  }

  private void AssertValidGetter(AccessorSyntax? accessor) {
    if (accessor is { AccessorType: not AccessorType.Get }) {
      throw new ArgumentException("Invalid accessor. Getter must be be of type get");
    }
  }

  private void AssertValidSetter(AccessorSyntax? accessor) {
    if (accessor is { AccessorType: not (AccessorType.Init or AccessorType.Set) }) {
      throw new ArgumentException("Invalid accessor. Setter must be either init or set");
    }
  }

  public override Syntax Clone() {
    AssertHasGetter();
    var clone = new AccessorListSyntax(Getter?.Clone<AccessorSyntax>(), Setter?.Clone<AccessorSyntax>());
    return clone;
  }
}