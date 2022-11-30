using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public sealed class PropertySyntax : PropertySyntax<object> {
  public PropertySyntax(Type type, string identifier, Syntax? parent) : base(type, identifier, parent) { }

  public override Syntax Clone() {
    var clone = new PropertySyntax(Type, Identifier, null);
    clone.CloneFrom(this);
    return clone;
  }
}

public class PropertySyntax<T> : MemberSyntax, IValueHolderSyntax<T> {
  public PropertySyntax(string identifier, Syntax parent) : this(typeof(T), identifier, parent) { }

  protected PropertySyntax(Type type, string identifier, Syntax? parent = null) : base(type, identifier, parent) {
    Accessors = new AccessorListSyntax(this);
  }

  protected internal override int Priority => 1;

  public AccessorListSyntax Accessors { get; }

  public T? Value { get; set; }

  // TODO: Add shortcut for backing field 
  public PropertySyntax<T> DefineBackingFieldGetter(FieldSyntax<T> field) {
    return this;
  }
  
  public PropertySyntax<T> DefineBackingFieldSetter(FieldSyntax<T> field) {
    return this;
  }
  
  public PropertySyntax<T> DefineAutoGetter() {
    Accessors.DefineGetter(AccessorSyntax.AutoGet);
    return this;
  }

  public PropertySyntax<T> DefineGetter(AccessorSyntax getter) {
    Accessors.DefineGetter(getter);
    return this;
  }

  public PropertySyntax<T> DefineAutoSetter() {
    Accessors.DefineSetter(AccessorSyntax.AutoSet);
    return this;
  }

  public PropertySyntax<T> DefineAutoInitSetter() {
    Accessors.DefineSetter(AccessorSyntax.AutoInit);
    return this;
  }

  public PropertySyntax<T> DefineSetter(AccessorSyntax setter) {
    Accessors.DefineSetter(setter);
    return this;
  }
  
  // TODO: Add custom block builder for properties (ImplicitSetterValue `value`)
  public PropertySyntax<T> DefineSetter(BlockBuilder setterBuilder) {
    var setter = new AccessorSyntax(AccessorType.Set, setterBuilder.Build(), SyntaxVisibility.None, this);
    Accessors.DefineSetter(setter);
    return this;
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.WriteLineStart();
    writer.Write(Visibility);

    writer.WriteTypeName(Type);
    writer.Write(" ");

    writer.Write(Identifier);
    writer.Write(" ");

    Accessors.WriteAsSyntax(writer);
  }

  public override Syntax Clone() {
    var clone = new PropertySyntax<T>(Type, Identifier);
    clone.CloneFrom(this);
    return clone;
  }

  protected void CloneFrom(PropertySyntax<T> other) {
    Accessors.DefineGetter(other.Accessors.Getter?.Clone<AccessorSyntax>());
    Accessors.DefineSetter(other.Accessors.Setter?.Clone<AccessorSyntax>());
  }
}