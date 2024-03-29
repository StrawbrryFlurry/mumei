﻿using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Tests.SyntaxNodes.Stubs;

public class StubMemberSyntax : MemberSyntax {
  public StubMemberSyntax(Type type, string identifier, Syntax parent) : base(type, identifier, parent) { }

  protected internal override int Priority { get; } = 10;

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }
}