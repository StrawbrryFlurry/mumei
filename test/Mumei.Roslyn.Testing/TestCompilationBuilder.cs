using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Mumei.Roslyn.Testing;

public sealed class TestCompilationBuilder {
  public const string DefaultAssemblyName = "Compilation_____Assembly";
  private readonly MetadataReferenceCollection _metadataReferences = new();

  private readonly SourceFileList _sources = new("", "cs");

  private Compilation? _compilation;

  public TestCompilationBuilder() {
    _metadataReferences.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
  }

  public Compilation Compilation => _compilation ??= CreateCompilation();

  private Compilation CreateCompilation() {
    var syntaxTrees = ParseSourceTexts();

    return CSharpCompilation.Create(
      DefaultAssemblyName,
      syntaxTrees,
      _metadataReferences,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
  }

  private SyntaxTree[] ParseSourceTexts() {
    var syntaxTrees = new SyntaxTree[_sources.Count];
    for (var i = 0; i < _sources.Count; i++) {
      syntaxTrees[i] = CSharpSyntaxTree.ParseText(_sources[i].content);
    }

    return syntaxTrees;
  }

  public TestCompilationBuilder AddTypeReference<TAssemblyType>() {
    _metadataReferences.Add(CreateMetadataReferenceFromAssemblyType<TAssemblyType>());
    return this;
  }

  private MetadataReference CreateMetadataReferenceFromAssemblyType<TAssemblyType>() {
    var typeInfo = typeof(TAssemblyType).GetTypeInfo();
    var assemblyLocation = typeInfo.Assembly.Location;
    return MetadataReference.CreateFromFile(assemblyLocation);
  }

  public TestCompilationBuilder AddSourceText(string sourceText) {
    _sources.Add(SourceText.From(sourceText));
    return this;
  }

  public Compilation Build() {
    return Compilation;
  }

  public static implicit operator Compilation(TestCompilationBuilder builder) {
    return builder.Build();
  }
}