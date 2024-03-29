﻿using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis;
using Xunit.Sdk;

namespace Mumei.Roslyn.Testing;

public static class SourceGeneratorTestExtensions {
  public static SourceGeneratorTestAssertions Should(this SourceGeneratorTestResult testResult) {
    return new SourceGeneratorTestAssertions(testResult);
  }
}

public sealed class SourceGeneratorTestAssertions
  : ReferenceTypeAssertions<SourceGeneratorTestResult, SourceGeneratorTestAssertions> {
  protected override string Identifier { get; } = "SourceGeneratorTestResult";

  private readonly SourceGeneratorTestResult _testResult;

  public SourceGeneratorTestAssertions(SourceGeneratorTestResult testResult) : base(testResult) {
    _testResult = testResult;
  }

  public SourceGeneratorGeneratedFileAssertions HaveGeneratedFile(string filePath) {
    var generatedFile = Subject.RunResult.GeneratedTrees.SingleOrDefault(t => t.FilePath.EndsWith(filePath));
    generatedFile.Should().NotBeNull();

    return new SourceGeneratorGeneratedFileAssertions(generatedFile!);
  }
}

public sealed class SourceGeneratorGeneratedFileAssertions
  : ReferenceTypeAssertions<SyntaxTree, SourceGeneratorGeneratedFileAssertions> {
  protected override string Identifier { get; } = "SourceGeneratorGeneratedFiles";

  public SourceGeneratorGeneratedFileAssertions(SyntaxTree tree) :
    base(tree) { }

  public AndConstraint<SourceGeneratorGeneratedFileAssertions> WithContent(
    string content,
    Action<SourceFileBuilder>? configure = null
  ) {
    var builder = new SourceFileBuilder(content);
    configure?.Invoke(builder);
    return AssertTextEqual(builder.ToString());
  }

  public AndConstraint<SourceGeneratorGeneratedFileAssertions> WithGeneratedContent(
    string content,
    Action<SourceFileBuilder>? configure = null
  ) {
    var builder = new SourceFileBuilder(content);
    builder.WithFileComment("<auto-generated/>");
    configure?.Invoke(builder);
    return AssertTextEqual(builder.ToString());
  }

  private AndConstraint<SourceGeneratorGeneratedFileAssertions> AssertTextEqual(string expected) {
    var actual = Subject.ToString();
    try {
      Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }
    catch (EqualException e) {
      var exceptionMessage = $"""
                              Expected content:
                              {expected}

                              Actual content:
                              {actual}

                              Difference:
                              {e.Message.Replace("Assert.Equal() Failure\r\n", "")}  
                              """;
      throw new XunitException(exceptionMessage);
    }

    return new AndConstraint<SourceGeneratorGeneratedFileAssertions>(this);
  }
}