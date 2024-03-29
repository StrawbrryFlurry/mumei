﻿using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.CodeGen.Tests.SyntaxNodes.Base;

public class VariableSyntaxTests {
  [Fact]
  public void VariableSyntax_WriteAsSyntax_WritesVariableNameAsString() {
    var sut = new VariableExpressionSyntax<string>("Test");

    sut.WriteSyntaxAsString().Should().Be("Test");
  }

  [Fact]
  public void VariableSyntax_WriteAsSyntax_WritesVariableNameAsString_WhenVariableIsNonGenericInstance() {
    var sut = new VariableExpressionSyntax(typeof(string), "Test");

    sut.WriteSyntaxAsString().Should().Be("Test");
  }

  [Fact]
  public void VariableDeclarationSyntax_WriteAsSyntax_WritesVariableTypeAndName_WhenVariableDoesNotHavInitializer() {
    var sut = new VariableDeclarationStatementSyntax(typeof(string), "Test");

    sut.WriteSyntaxAsString().Should().Be("String Test;");
  }

  [Fact]
  public void
    VariableDeclarationSyntax_WriteAsSyntax_WritesVariableTypeNameAndInitializer_WhenVariableHasInitializer() {
    var sut = new VariableDeclarationStatementSyntax(typeof(string), "Test", Expression.Constant(10));

    sut.WriteSyntaxAsString().Should().Be("String Test = 10;");
  }

  [Fact]
  public void
    VariableDeclarationSyntax_Clone_ReturnsNewInstanceWithCloneOfTheInitializer() {
    var sut = new VariableDeclarationStatementSyntax(typeof(string), "Test", Expression.Constant(10));

    var clone = (VariableDeclarationStatementSyntax)sut.Clone();

    clone.Initializer.Should().NotBe(sut.Initializer);
    clone.Initializer.Should().NotBe(sut.Initializer);
    clone.Identifier.Should().Be(sut.Identifier);
    clone.Type.Should().Be(sut.Type);
  }
}