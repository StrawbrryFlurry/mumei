﻿using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class BlockSyntaxBuilderTests {
  [Fact]
  public void VariableDeclaration_ReturnsVariableExpressionWithDefaultValue() {
    var sut = new BlockSyntaxBuilder();

    var v = (VariableExpressionSyntax<int>)sut.VariableDeclaration(typeof(int), "Test");

    v.Value.Should().Be(0);
  }

  [Fact]
  public void VariableDeclaration_AddsVariableDeclarationToStatementList() {
    var sut = new BlockSyntaxBuilder();

    sut.VariableDeclaration(typeof(string), "Test");
    var declaration = sut.Statements.OfType<VariableDeclarationStatementSyntax>().First();

    declaration.Identifier.Should().Be("Test");
    declaration.Type.Should().Be<string>();
  }

  [Fact]
  public void VariableDeclaration_AddsVariableDeclarationToStatementList_WhenDeclarationHasInitializer() {
    var sut = new BlockSyntaxBuilder();

    sut.VariableDeclaration(typeof(string), "Test", Expression.Constant(42));
    var declaration = sut.Statements.OfType<VariableDeclarationStatementSyntax>().First();

    declaration.Initializer!.ToString().Should().Be("42");
  }

  [Fact]
  public void VariableDeclaration_T_ReturnsVariableExpressionWithDefaultValue() {
    var sut = new BlockSyntaxBuilder();

    var v = sut.VariableDeclaration<string>("Test");

    v.Value.Should().BeNull();
  }

  [Fact]
  public void VariableDeclaration_T_AddsVariableDeclarationToStatementList() {
    var sut = new BlockSyntaxBuilder();

    sut.VariableDeclaration<string>("Test");
    var declaration = sut.Statements.OfType<VariableDeclarationStatementSyntax>().First();

    declaration.Identifier.Should().Be("Test");
    declaration.Type.Should().Be<string>();
  }

  [Fact]
  public void VariableDeclaration_T_AddsVariableDeclarationToStatementList_WhenDeclarationHasInitializer() {
    var sut = new BlockSyntaxBuilder();

    sut.VariableDeclaration<string>("Test", Expression.Constant(42));
    var declaration = sut.Statements.OfType<VariableDeclarationStatementSyntax>().First();

    declaration.Initializer!.ToString().Should().Be("42");
  }
}