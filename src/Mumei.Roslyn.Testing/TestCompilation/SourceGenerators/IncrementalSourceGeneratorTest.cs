using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Mumei.Roslyn.Testing;

public sealed class IncrementalSourceGeneratorTest<TGenerator>(Compilation compilation) where TGenerator : IIncrementalGenerator, new() {
    private GeneratorDriver _driver = CSharpGeneratorDriver.Create(new TGenerator())
        .WithUpdatedParseOptions((CSharpParseOptions) compilation.SyntaxTrees.First().Options!);

    private Compilation _currentCompilation = compilation;

    public IncrementalSourceGeneratorTest<TGenerator> RunWithAssert(Action<Result> assertResult) {
        _driver = _driver.RunGeneratorsAndUpdateCompilation(
            _currentCompilation,
            out var updatedCompilation,
            out var diagnostics
        );
        var runResult = _driver.GetRunResult();

        foreach (var generatorResult in runResult.Results) {
            Assert.Null(generatorResult.Exception);
        }

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

        var result = new Result(
            runResult,
            updatedCompilation
        );
        assertResult(result);
        _currentCompilation = updatedCompilation;

        return this;

        static string GetSourceTextLocationWithContext(SyntaxTree tree, Location location) {
            var text = tree.GetText();
            var contextStart = Math.Max(0, location.SourceSpan.Start - 20);
            var contextLength = Math.Min(20, location.SourceSpan.Start - contextStart);

            var endContextLength = Math.Min(20, text.Length - location.SourceSpan.End);
            var totalLength = contextLength + location.SourceSpan.Length + endContextLength;

            return text.GetSubText(new TextSpan(contextStart, totalLength)).ToString();
        }

    }

    public IncrementalSourceGeneratorTest<TGenerator> UpdateCompilation(Action<UpdateCompilationContext> updateCompilation) {
        return this;
    }

    public sealed class Result(
        GeneratorDriverRunResult result,
        Compilation compilation
    ) {
        public Compilation Compilation => compilation;

        public ImmutableArray<Diagnostic> Diagnostics => result.Diagnostics;
        public ImmutableArray<SyntaxTree> GeneratedTrees => result.GeneratedTrees;
    }

    public sealed class UpdateCompilationContext {
        public Compilation Compilation { get; init; }
        private readonly List<Func<Compilation, Compilation>> _updates = new();

        public void UpdateFile(string name, string newText) { }

        public void UpdateCompilation(Func<Compilation, Compilation> updateCompilation) {
            _updates.Add(updateCompilation);
        }
    }
}