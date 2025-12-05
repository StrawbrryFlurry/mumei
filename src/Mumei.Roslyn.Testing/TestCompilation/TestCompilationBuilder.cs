using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.Roslyn.Testing;

public sealed class TestCompilationBuilder {
    public const string DefaultAssemblyName = "TestAssembly";

    private readonly MetadataReferenceCollection _metadataReferences = new();
    private readonly List<SyntaxTree> _sources = new();

    private string _assemblyName = DefaultAssemblyName;

    private readonly List<string> _interceptorsAllowedIn = new();

    private string? _sourceNamespace;
    public string? SourceNamespace => _sourceNamespace;

    private CSharpParseOptions ParseOptions => field ??= new CSharpParseOptions(LanguageVersion.Preview)
        .WithFeatures([
            MakeInterceptorsNamespacesFeature()
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
        AddGlobalUsings();
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

    private void AddGlobalUsings() {
        const string globalUsings = """
                                    global using System;
                                    global using System.Collections.Generic;
                                    global using System.IO;
                                    global using System.Linq;
                                    global using System.Net.Http;
                                    global using System.Threading;
                                    global using System.Threading.Tasks;
                                    """;

        _sources.Add(
            CSharpSyntaxTree.ParseText(
                globalUsings,
                path: "__GlobalUsings.cs"
            )
        );
    }

    public TestCompilationBuilder AddReference(ICompilationReference reference) {
        if (reference is IRootCompilationReference rootReference) {
            _sourceNamespace = rootReference.SourceNamespace;
        }

        reference.AddToCompilation(_sources, _metadataReferences);
        return this;
    }

    public TestCompilationBuilder AllowInterceptorsIn(string name) {
        _interceptorsAllowedIn.Add(name);
        return this;
    }

    private KeyValuePair<string, string> MakeInterceptorsNamespacesFeature() {
        _interceptorsAllowedIn.Add(_assemblyName);
        _interceptorsAllowedIn.Add($"{_assemblyName}.Generated");
        _interceptorsAllowedIn.Add("Generated");

        var namespaces = string.Join(";", _interceptorsAllowedIn);
        return new KeyValuePair<string, string>("InterceptorsNamespaces", namespaces);
    }
}