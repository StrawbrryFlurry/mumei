using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.Roslyn.Testing;

public sealed class TestCompilationBuilder {
    public const string DefaultAssemblyName = "TestAssembly";

    private readonly MetadataReferenceCollection _metadataReferences = new();
    private readonly List<SyntaxTree> _sources = new();

    private string _assemblyName = DefaultAssemblyName;

    private CSharpParseOptions ParseOptions => new CSharpParseOptions(LanguageVersion.Preview)
        .WithFeatures([
            new KeyValuePair<string, string>("InterceptorsNamespaces", $"{_assemblyName}.Generated;Generated")
        ]);

    public TestCompilationBuilder WithAssemblyName(string assemblyName) {
        _assemblyName = assemblyName;
        return this;
    }

    public TestCompilationBuilder AddTypeReference<TAssemblyType>() {
        _metadataReferences.AddReference<TAssemblyType>();
        return this;
    }

    public TestCompilationBuilder AddAssemblyReference(string assemblyName) {
        _metadataReferences.AddReference(assemblyName);
        return this;
    }

    public TestCompilationBuilder AddSource(string fileName, string content) {
        _sources.Add(CSharpSyntaxTree.ParseText(content, path: fileName));
        return this;
    }

    public TestCompilationBuilder AddSource(string content) {
        _sources.Add(CSharpSyntaxTree.ParseText(content, path: Guid.NewGuid().ToString("N")));
        return this;
    }

    public TestCompilationBuilder AddSource([StringSyntax("cs")] CommonSyntaxStringInterpolationHandler content) {
        _sources.Add(CSharpSyntaxTree.ParseText(content.ToString(), path: Guid.NewGuid().ToString("N")));
        return this;
    }

    public TestCompilationBuilder AddSource(string content, Action<SourceFileBuilder> configure) {
        var sourceFileBuilder = new SourceFileBuilder(content);
        configure?.Invoke(sourceFileBuilder);

        _metadataReferences.AddReferences(sourceFileBuilder.Usings);
        _sources.Add(sourceFileBuilder.ToSyntaxTree());

        return this;
    }

    public TestCompilationBuilder AddSources(string[] sources) {
        _sources.AddRange(sources.Select(f => CSharpSyntaxTree.ParseText(f, path: Guid.NewGuid().ToString("N"))));
        return this;
    }

    public Compilation Build() {
        return CreateCompilation();
    }

    private Compilation CreateCompilation() {
        UpdateSyntaxTreesWithParseOptions();
        return CSharpCompilation.Create(
            _assemblyName,
            _sources,
            _metadataReferences.MetadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    private void UpdateSyntaxTreesWithParseOptions() {
        for (var i = 0; i < _sources.Count; i++) {
            var syntaxTree = _sources[i];
            var syntaxRootNode = (CSharpSyntaxNode) syntaxTree.GetRoot();
            var updatedTree = CSharpSyntaxTree.Create(syntaxRootNode, ParseOptions, syntaxTree.FilePath);
            _sources[i] = updatedTree;
        }
    }

    public TestCompilationBuilder AddReference(ICompilationReference reference) {
        reference.AddToCompilation(_sources, _metadataReferences);
        return this;
    }
}