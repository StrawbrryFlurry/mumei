﻿using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class PropertySyntax : PropertySyntax<object> {
  public PropertySyntax(Type type, string identifier, Syntax parent) : base(type, identifier, parent) { }
}

public class PropertySyntax<T> : MemberSyntax, IValueHolderSyntax<T> {
  public PropertySyntax(string identifier, Syntax parent) : base(typeof(T), identifier, parent) { }

  protected PropertySyntax(Type type, string identifier, Syntax parent) : base(type, identifier, parent) { }

  public override int Priority => 1;

  public AccessorListSyntax Accessors { get; } = new();

  public T? Value { get; set; }

  public void DefineAutoGetter() {
    Accessors.DefineGetter(AccessorSyntax.AutoGet);
  }

  public void DefineGetter(AccessorSyntax getter) {
    Accessors.DefineGetter(getter);
  }

  public void DefineAutoSetter() {
    Accessors.DefineSetter(AccessorSyntax.AutoSet);
  }

  public void DefineAutoInitSetter() {
    Accessors.DefineSetter(AccessorSyntax.AutoInit);
  }

  public void DefineSetter(AccessorSyntax setter) {
    Accessors.DefineSetter(setter);
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.WriteTypeName(Type);
    writer.Write(" ");
    writer.Write(Identifier);
    writer.Write(" ");
    Accessors.WriteAsSyntax(writer);
  }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }
}