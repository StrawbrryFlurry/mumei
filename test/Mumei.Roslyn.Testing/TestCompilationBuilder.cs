using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.Roslyn.Testing;

public sealed class TestCompilationBuilder {
  public const string DefaultAssemblyName = "Compilation_____Assembly";

  private readonly MetadataReferenceCollection _metadataReferences = new();
  private readonly List<SyntaxTree> _sources = new();

  private string _assemblyName = DefaultAssemblyName;
  private Compilation? _compilation;

  public Compilation Compilation => _compilation ??= CreateCompilation();

  public TestCompilationBuilder WithAssemblyName(string assemblyName) {
    _assemblyName = assemblyName;
    return this;
  }

  public TestCompilationBuilder AddTypeReference<TAssemblyType>() {
    _metadataReferences.AddReference<TAssemblyType>();
    return this;
  }

  public TestCompilationBuilder AddSource(string fileName, string content) {
    _sources.Add(CSharpSyntaxTree.ParseText(content, path: fileName));
    return this;
  }

  public TestCompilationBuilder AddSource(string source, Action<SourceFileBuilder>? configure = null) {
    var sourceFileBuilder = new SourceFileBuilder(source);
    configure?.Invoke(sourceFileBuilder);

    _metadataReferences.AddReferences(sourceFileBuilder.Usings);
    _sources.Add(sourceFileBuilder.ToSyntaxTree());

    return this;
  }

  public TestCompilationBuilder AddSources(string[] sources) {
    _sources.AddRange(sources.Select(f => CSharpSyntaxTree.ParseText(f)));
    return this;
  }

  public Compilation Build() {
    return Compilation;
  }

  private Compilation CreateCompilation() {
    return CSharpCompilation.Create(
      _assemblyName,
      _sources,
      _metadataReferences.MetadataReferences
    );
  }

  public static implicit operator Compilation(TestCompilationBuilder builder) {
    return builder.Build();
  }
}