using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.Setup;

public sealed class TestCompilationBuilder {
    public const string DefaultAssemblyName = "Compilation_____Assembly";

    private readonly MetadataReferenceCollection _metadataReferences = new();
    private readonly List<SyntaxTree> _sources = new();

    private string _assemblyName = DefaultAssemblyName;
    private Compilation? _compilation;

    public Compilation Compilation => _compilation ??= CreateCompilation();

    public CSharpParseOptions ParseOptions => new CSharpParseOptions(LanguageVersion.CSharp13)
        .WithFeatures([
            new KeyValuePair<string, string>("InterceptorsNamespaces", $"{_assemblyName}.Paramita.Generated")
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

    public TestCompilationBuilder AddSource(string source, Action<SourceFileBuilder>? configure = null) {
        var sourceFileBuilder = new SourceFileBuilder(source);
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
        return Compilation;
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
            var syntaxRootNode = (CSharpSyntaxNode)syntaxTree.GetRoot();
            var updatedTree = CSharpSyntaxTree.Create(syntaxRootNode, ParseOptions, syntaxTree.FilePath);
            _sources[i] = updatedTree;
        }
    }

    public static implicit operator Compilation(TestCompilationBuilder builder) {
        return builder.Build();
    }

    internal TestCompilationBuilder AddReference(ITypeRef reference, HashSet<string>? seenTexts = null) {
        seenTexts ??= [];
        if (reference is SourceCodeTypeRef sctr) {
            if (seenTexts.Add(sctr.SourceCode)) {
                AddSource(sctr.SourceCode);
            }

            foreach (var innerRefs in sctr.References) {
                AddReference(innerRefs, seenTexts);
            }

            return this;
        }

        if (reference is AssemblyTypeRef atr) {
            AddAssemblyReference(atr.AssemblyName);
        }

        return this;
    }
}