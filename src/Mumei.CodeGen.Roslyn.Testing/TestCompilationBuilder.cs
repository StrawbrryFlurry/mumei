using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.CodeGen.Roslyn.Testing;

public sealed class TestCompilationBuilder {
    public const string DefaultAssemblyName = "TestAssembly";

    private static Dictionary<string, MetadataReference>? _defaultMetadataReferences;
    private readonly List<string> _interceptorsAllowedIn = new();

    private readonly Dictionary<string, MetadataReference> _metadataReferences = new();
    private readonly List<SyntaxTree> _sources = new();
    private readonly List<(CodeFragment Fragment, bool Async)> _fragments = [];

    private readonly List<string> _supressedDiagnosticIds = new();

    private string _assemblyName = DefaultAssemblyName;

    private bool _emulateGlobalUsings = true;

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
        AddAssemblyReference(t.Assembly);
        return this;
    }

    public TestCompilationBuilder WithIgnoredDiagnostic(params string[] diagnostics) {
        _supressedDiagnosticIds.AddRange(diagnostics);
        return this;
    }

    public TestCompilationBuilder AddAssemblyReference(Assembly assembly) {
        var assemblyName = assembly.GetName().FullName;
        if (!HasMetadataReference(assemblyName)) {
            _metadataReferences[assemblyName] =
                MetadataReference.CreateFromFile(assembly.Location);
        }

        return this;
    }

    private bool HasMetadataReference(string key) {
        return _metadataReferences.ContainsKey(key);
    }

    public TestCompilationBuilder AddSource([StringSyntax("cs")] CodeFragment sourceFile) {
        return AddSource(sourceFile.ToString());
    }


    public TestCompilationBuilder AddFragment([StringSyntax("cs")] CodeFragment fragment, bool async = false) {
        _fragments.Add((fragment, async));
        return this;
    }

    public TestCompilationBuilder AddSource([StringSyntax("cs")] string sourceFile) {
        return AddSource($"TestSourceFile_{Guid.NewGuid():N}", sourceFile);
    }

    public TestCompilationBuilder AddSource(string fileName, [StringSyntax("cs")] CodeFragment sourceFile) {
        return AddSource(fileName, sourceFile.ToString());
    }

    public TestCompilationBuilder AddSource(string fileName, [StringSyntax("cs")] string sourceFile) {
        _sources.Add(CSharpSyntaxTree.ParseText(sourceFile, path: fileName));
        return this;
    }

    public TestCompilationBuilder AddSource(DeclarationSourceComponent file) {
        file.Register(this);
        return this;
    }

    public TestCompilationBuilder WithoutGlobalUsings() {
        _emulateGlobalUsings = false;
        return this;
    }

    public Compilation Build() {
        return CreateCompilation();
    }

    private Compilation CreateCompilation() {
        AddGlobalUsings();
        EmitFragmentsAsSource();
        UpdateSyntaxTreesWithParseOptions();
        IncludeAppDomainMetadataReferences();

        return CSharpCompilation.Create(
            _assemblyName,
            _sources,
            _metadataReferences.Values,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable,
                specificDiagnosticOptions: _supressedDiagnosticIds.Select(x => new KeyValuePair<string, ReportDiagnostic>(x, ReportDiagnostic.Suppress))
            )
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
        if (!_emulateGlobalUsings) {
            return;
        }

        const string globalUsings = """
                                    #pragma warning disable CS8019
                                    // <auto-generated/>

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
                path: "__GlobalUsings.g.cs"
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

    private void EmitFragmentsAsSource() {
        if (_fragments.Count == 0) {
            return;
        }

        var fragmentsFile = new StringBuilder();
        fragmentsFile.AppendLine(
            """
            public static class __FragmentOutput {
            """
        );

        var fragmentId = 0;
        foreach (var codeFragment in _fragments) {
            var asyncModifier = codeFragment.Async ? "async " : "";
            var returnType = codeFragment.Async ? typeof(Task).FullName : "void";
            fragmentsFile.AppendLine($$"""public static{{asyncModifier}} {{returnType}} __GeneratedFragmentMethod_{{fragmentId}}() {""");
            fragmentsFile.AppendLine(codeFragment.Fragment.ToString());
            fragmentsFile.AppendLine("}");
            fragmentId++;
        }

        fragmentsFile.Append('}');

        AddSource("CompilationFragments.g.cs", fragmentsFile.ToString());
    }
}