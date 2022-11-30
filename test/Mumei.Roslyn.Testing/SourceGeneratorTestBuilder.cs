using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Mumei.Roslyn.Testing.Abstractions;

namespace Mumei.Roslyn.Testing;

public sealed class SourceGeneratorTestBuilder<TSourceGenerator> where TSourceGenerator : ISourceGenerator, new() {
  private readonly SourceFileCollection _generatedSources = new();
  private readonly SourceFileList _sources = new("", "cs");
  private readonly MetadataReferenceCollection _references = new();
  
  public SourceGeneratorTestBuilder<TSourceGenerator> AddTypeReference<TAssemblyType>() {
    _references.Add(CreateMetadataReferenceFromAssemblyType<TAssemblyType>());
    return this;
  }
  
  private MetadataReference CreateMetadataReferenceFromAssemblyType<TAssemblyType>() {
    var typeInfo = typeof(TAssemblyType).GetTypeInfo();
    var assemblyLocation = typeInfo.Assembly.Location;
    return MetadataReference.CreateFromFile(assemblyLocation);
  }
  
  public SourceGeneratorTestBuilder<TSourceGenerator> AddSource(string fileName, string source) {
    _sources.Add((fileName, source));
    return this;
  }

  public SourceGeneratorTestBuilder<TSourceGenerator> AddSource(string source) {
    var fileName = Path.GetRandomFileName();
    return AddSource(fileName, source);
  }

  public SourceGeneratorTestBuilder<TSourceGenerator> AddGeneratedSource(string fileName, string source) {
    _generatedSources.Add((fileName, source));
    return this;
  }

  public CSharpSourceGeneratorTest<TSourceGenerator> Build() {
    var test = new CSharpSourceGeneratorTest<TSourceGenerator>(_references);
    test.TestState.Sources.AddRange(_sources);
    test.TestState.GeneratedSources.AddRange(_generatedSources);
    return test;
  }
}

public sealed class CSharpSourceGeneratorTest<TSourceGenerator> 
  : CSharpSourceGeneratorTest<TSourceGenerator, XUnitVerifier> where TSourceGenerator : ISourceGenerator, new() {
  private readonly MetadataReferenceCollection _references;

  public CSharpSourceGeneratorTest(MetadataReferenceCollection references) {
    _references = references;
  }
  
  protected override Project ApplyCompilationOptions(Project project) {
    project = project.AddMetadataReferences(_references);
    return base.ApplyCompilationOptions(project);
  }
}