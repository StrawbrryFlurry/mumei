using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.Roslyn.Testing;

public sealed class SourceGeneratorTest<TSourceGenerator> where TSourceGenerator : IIncrementalGenerator, new() {
  private readonly Compilation _compilation;

  public SourceGeneratorTest(Compilation compilation) {
    _compilation = compilation;
  }

  public SourceGeneratorTest(Action<TestCompilationBuilder> configureCompilationAction) {
    var compilationBuilder = new TestCompilationBuilder();
    configureCompilationAction(compilationBuilder);
    _compilation = compilationBuilder.Build();
  }

  public SourceGeneratorTest(params string[] sources) {
    var compilationBuilder = new TestCompilationBuilder();
    compilationBuilder.AddSources(sources);
    _compilation = compilationBuilder.Build();
  }

  public SourceGeneratorTestResult Run() {
    var driver = CSharpGeneratorDriver.Create(new TSourceGenerator());

    var runResult = driver.RunGenerators(_compilation).GetRunResult();

    return new SourceGeneratorTestResult(runResult);
  }
}

public sealed class SourceGeneratorTestResult {
  public GeneratorDriverRunResult RunResult { get; }

  public SourceGeneratorTestResult(
    GeneratorDriverRunResult runResult
  ) {
    RunResult = runResult;
  }
}