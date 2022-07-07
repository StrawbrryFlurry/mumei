using Mumei.CodeGen.SyntaxBuilders;

namespace Mumei.CodeGen.SyntaxNodes;

public abstract class MemberSyntaxBuilder<TMemberSyntax> : SyntaxBuilder<TMemberSyntax>
  where TMemberSyntax : MemberSyntax {
  protected new readonly TypeSyntax Parent;
  protected readonly Type? Type;

  public MemberSyntaxBuilder(string name, Type? type, TypeSyntax parent) : base(name, parent) {
    Parent = parent;
    Type = type;
  }

  protected override TMemberSyntax MakeSyntaxInstance() {
    var instance = base.MakeSyntaxInstance();
    instance.Type = Type;

    return instance;
  }

  /// <summary>
  ///   Adds the built member to the parent.
  /// </summary>
  /// <returns>The member instance</returns>
  public MemberSyntax Add() {
    var member = (MemberSyntax) Build();
    Parent.AddMember(member);
    return member;
  }
}