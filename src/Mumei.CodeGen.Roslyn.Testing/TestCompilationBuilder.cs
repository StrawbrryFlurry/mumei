using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.CodeGen.Roslyn.Testing;

public sealed class TestCompilationBuilder {
    public const string DefaultAssemblyName = "TestAssembly";

    private static Dictionary<string, MetadataReference>? _defaultMetadataReferences;

    private readonly Dictionary<string, MetadataReference> _metadataReferences = new();
    private readonly List<SyntaxTree> _sources = new();

    private string _assemblyName = DefaultAssemblyName;

    private readonly List<string> _interceptorsAllowedIn = new();

    public CSharpParseOptions ParseOptions => field ??= new CSharpParseOptions(LanguageVersion.Preview)
        .WithFeatures([
            MakeInterceptorsNamespacesFeature()
        ]);

    public TestCompilationBuilder WithAssemblyName(string assemblyName) {
        _assemblyName = assemblyName;
        return this;
    }


    public TestCompilationBuilder AddTypeReference<TAssemblyType>() {
        return AddTypeReference(typeof(TAssemblyType));
    }

    public TestCompilationBuilder AddTypeReference(Type t) {
        var assemblyName = t.Assembly.GetName().FullName;
        if (!HasMetadataReference(assemblyName)) {
            _metadataReferences[assemblyName] =
                MetadataReference.CreateFromFile(t.Assembly.Location);
        }

        return this;
    }

    private bool HasMetadataReference(string key) {
        return _metadataReferences.ContainsKey(key);
    }

    public TestCompilationBuilder AddSource(string fileName, [StringSyntax("cs")] CodeFragment sourceFile) {
        _sources.Add(CSharpSyntaxTree.ParseText(sourceFile.ToString(), path: fileName));
        return this;
    }

    public TestCompilationBuilder AddSource([StringSyntax("cs")] CodeFragment sourceFile) {
        _sources.Add(CSharpSyntaxTree.ParseText(sourceFile.ToString(), path: $"TestSourceFile_{Guid.NewGuid():N}"));
        return this;
    }

    public Compilation Build() {
        return CreateCompilation();
    }

    private Compilation CreateCompilation() {
        AddGlobalUsings();
        UpdateSyntaxTreesWithParseOptions();
        IncludeAppDomainMetadataReferences();

        return CSharpCompilation.Create(
            _assemblyName,
            _sources,
            _metadataReferences.Values,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    private void UpdateSyntaxTreesWithParseOptions() {
        for (var i = 0; i < _sources.Count; i++) {
            var syntaxTree = _sources[i];
            var syntaxRootNode = (CSharpSyntaxNode)syntaxTree.GetRoot();
            var updatedTree = CSharpSyntaxTree.Create(syntaxRootNode, ParseOptions, syntaxTree.FilePath);
            _sources[i] = updatedTree;
        }
    }

    private void IncludeAppDomainMetadataReferences() {
        if (_defaultMetadataReferences is null) {
            _defaultMetadataReferences = new Dictionary<string, MetadataReference>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location)) {
                    continue;
                }

                _defaultMetadataReferences.Add(
                    assembly.Location,
                    MetadataReference.CreateFromFile(assembly.Location)
                );
            }
        }

        foreach (var (assemblyName, reference) in _defaultMetadataReferences) {
            _metadataReferences[assemblyName] = reference;
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