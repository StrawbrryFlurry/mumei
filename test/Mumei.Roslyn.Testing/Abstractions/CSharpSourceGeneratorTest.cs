using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace Mumei.Roslyn.Testing.Abstractions;

public class CSharpSourceGeneratorTest<TSourceGenerator, TVerifier> : SourceGeneratorTest<TVerifier>
  where TSourceGenerator : ISourceGenerator, new()
  where TVerifier : IVerifier, new() {
  public const LanguageVersion DefaultLanguageVersion = LanguageVersion.Latest;
  protected override string DefaultFileExt => "cs";
  
  public override string Language => LanguageNames.CSharp;

  public CSharpSourceGeneratorTest() {
    ReferenceAssemblies = ReferenceAssemblies
      .AddAssemblies(ImmutableArray.Create("Mumei.CodeGen"));
  }
  
  protected override IEnumerable<ISourceGenerator> GetSourceGenerators() {
    return new ISourceGenerator[] { new TSourceGenerator() };
  }

  protected override GeneratorDriver CreateGeneratorDriver(
    Project project,
    ImmutableArray<ISourceGenerator> sourceGenerators
    ) {
    return CSharpGeneratorDriver.Create(
      sourceGenerators,
      project.AnalyzerOptions.AdditionalFiles,
      (CSharpParseOptions)project.ParseOptions!,
      project.AnalyzerOptions.AnalyzerConfigOptionsProvider);
  }
  
  protected override CompilationOptions CreateCompilationOptions() {
    return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);
  }

  protected override ParseOptions CreateParseOptions() {
    return new CSharpParseOptions(DefaultLanguageVersion, DocumentationMode.Diagnose);
  }
}