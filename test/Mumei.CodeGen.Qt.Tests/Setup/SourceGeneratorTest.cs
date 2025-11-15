using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.Setup;

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

    internal SourceGeneratorTest(ITypeRef reference) {
        var compilationBuilder = new TestCompilationBuilder();
        compilationBuilder.AddReference(reference);
        _compilation = compilationBuilder.Build();
    }

    public SourceGeneratorTest(params string[] sources) {
        var compilationBuilder = new TestCompilationBuilder();
        compilationBuilder.AddSources(sources);
        _compilation = compilationBuilder.Build();
    }

    public SourceGeneratorTestResult Run() {
        var driver = CSharpGeneratorDriver.Create(
            [new TSourceGenerator().AsSourceGenerator()],
            null,
            _compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
        );

        var runResult = driver.RunGeneratorsAndUpdateCompilation(
            _compilation,
            out var updatedCompilation,
            out var diagnostics
        ).GetRunResult();

        Assert.Empty(diagnostics);
        Assert.Empty(runResult.Diagnostics);

        const string unnecessaryUsingDirectiveId = "CS8019";
        var compilationDiagnostics = updatedCompilation.GetDiagnostics().Where(x => x.Id != unnecessaryUsingDirectiveId).ToArray();

        // Debug References
        var firstGeneratedTree = runResult.GeneratedTrees.FirstOrDefault();
        var firstDiagnostic = compilationDiagnostics.FirstOrDefault();

        if (compilationDiagnostics.Any()) {

            Assert.Multiple(compilationDiagnostics.Select<Diagnostic, Action>(diagnostic => {
                    var source = diagnostic.Location.SourceTree;
                    return () => Assert.Fail(
                        $"Diagnostic: {diagnostic.Id}:\n{diagnostic.GetMessage()}\nat: {GetSourceTextLocationWithContext(source, diagnostic.Location)}\nof: {source.GetText()}"
                    );
                }
            ).ToArray());
        }

        return new SourceGeneratorTestResult(runResult, updatedCompilation);

        static string GetSourceTextLocationWithContext(SyntaxTree tree, Location location) {
            var text = tree.GetText();
            var contextStart = Math.Max(0, location.SourceSpan.Start - 20);
            var contextLength = Math.Min(20, location.SourceSpan.Start - contextStart);

            var endContextLength = Math.Min(20, text.Length - location.SourceSpan.End);
            var totalLength = contextLength + location.SourceSpan.Length + endContextLength;

            return text.GetSubText(new TextSpan(contextStart, totalLength)).ToString();
        }
    }
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(SourceGeneratorTestDebugView))]
public sealed class SourceGeneratorTestResult(
    GeneratorDriverRunResult runResult,
    Compilation compilation
) {
    public ImmutableArray<SyntaxTree> GeneratedTrees => runResult.GeneratedTrees;
    private string DebuggerDisplay => $"{GeneratedTrees.Length} Generated Trees";

    public Compilation Compilation => compilation;

    private sealed class SourceGeneratorTestDebugView {
        private readonly SourceGeneratorTestResult _sourceGeneratorTestResult;

        public SourceGeneratorTestDebugView(SourceGeneratorTestResult sourceGeneratorTestResult) {
            _sourceGeneratorTestResult = sourceGeneratorTestResult;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Dictionary<string, string> Items {
            get { return _sourceGeneratorTestResult.GeneratedTrees.ToDictionary(x => x.FilePath, x => x.GetText().ToString()); }
        }
    }
}