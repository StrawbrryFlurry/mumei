using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Mumei.CodeGen.Roslyn.Testing;

public sealed class GeneratorTest(
    TestCompilationBuilder builder,
    IIncrementalGenerator[] generators
) {
    private TestCompilationBuilder CompilationBuilder => builder;
    private GeneratorDriver _driver = CreateDriver(builder.ParseOptions, generators);

    private Compilation _compilation = builder.Build();

    public static GeneratorTest For<TGenerator>()
        where TGenerator : IIncrementalGenerator, new() {
        return new GeneratorTest(new TestCompilationBuilder(), [new TGenerator()]);
    }

    public static GeneratorTest For<TGenerator>(params CodeFragment[] sourceFiles)
        where TGenerator : IIncrementalGenerator, new() {
        var compilationBuilder = new TestCompilationBuilder();
        foreach (var file in sourceFiles) {
            compilationBuilder.AddSource(file);
        }

        return new GeneratorTest(compilationBuilder, [new TGenerator()]);
    }

    public static GeneratorTest For<TGenerator>(Action<TestCompilationBuilder> configureCompilation)
        where TGenerator : IIncrementalGenerator, new() {
        var compilationBuilder = new TestCompilationBuilder();
        configureCompilation(compilationBuilder);
        return new GeneratorTest(compilationBuilder, [new TGenerator()]);
    }

    public GeneratorTest RunWithAssert(Action<Result> assertResult) {
        _driver = _driver.RunGeneratorsAndUpdateCompilation(
            _compilation,
            out var updatedCompilation,
            out var diagnostics
        );

        var runResult = _driver.GetRunResult();
        foreach (var generatorResult in runResult.Results) {
            Assert.Null(generatorResult.Exception);
        }

        Assert.Empty(diagnostics);
        Assert.Empty(runResult.Diagnostics);

        const string duplicateGlobalUsingDirectiveId = "CS8933";
        const string unnecessaryUsingDirectiveId = "CS8019";
        var compilationDiagnostics = updatedCompilation.GetDiagnostics().Where(x =>
            x.Id is not (unnecessaryUsingDirectiveId or duplicateGlobalUsingDirectiveId)
        ).ToArray();

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

    private static GeneratorDriver CreateDriver(
        CSharpParseOptions compilationParseOptions,
        IIncrementalGenerator[] generators
    ) {
        return CSharpGeneratorDriver.Create(
                generators.Select(x => x.AsSourceGenerator()),
                driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true))
            .WithUpdatedParseOptions(compilationParseOptions);
    }

    public GeneratorTest UpdateCompilation(
        Action<UpdateCompilationContext> updateCompilation) {
        var context = new UpdateCompilationContext();
        updateCompilation(context);
        _compilation = context.GetUpdateCompilation(_compilation);
        return this;
    }

    public sealed class Result(
        GeneratorDriverRunResult result,
        Compilation compilation
    ) {
        public Compilation Compilation => compilation;

        public ImmutableArray<Diagnostic> Diagnostics => result.Diagnostics;
        public ImmutableArray<SyntaxTree> GeneratedTrees => result.GeneratedTrees;
        public GeneratorDriverRunResult RunResult => result;

        /// <summary>Why each source output was produced in this run, e.g. <see cref="IncrementalStepRunReason.Cached"/> if nothing changed.</summary>
        public ImmutableArray<IncrementalStepRunReason> OutputReasons => [
            .. result.Results
                .SelectMany(x => x.TrackedOutputSteps)
                .SelectMany(x => x.Value)
                .SelectMany(x => x.Outputs)
                .Select(x => x.Reason)
        ];
    }

    public sealed class UpdateCompilationContext {
        private readonly List<Func<Compilation, Compilation>> _updates = new();

        internal Compilation GetUpdateCompilation(Compilation compilation) {
            var updatedCompilation = compilation;
            foreach (var update in _updates) {
                updatedCompilation = update(updatedCompilation);
            }

            return updatedCompilation;
        }

        public void UpdateFile(string name, string newText) {
            UpdateFile(name, _ => newText);
        }

        public void UpdateFile(string name, Func<string, string> updateText) {
            _updates.Add(c => {
                var tree = c.SyntaxTrees.First(t => t.FilePath.StartsWith(name));
                var newTree = CSharpSyntaxTree.ParseText(
                    SourceText.From(updateText(tree.GetText().ToString())),
                    (CSharpParseOptions)tree.Options,
                    tree.FilePath
                );

                return c.ReplaceSyntaxTree(tree, newTree);
            });
        }

        public void UpdateCompilation(Func<Compilation, Compilation> updateCompilation) {
            _updates.Add(updateCompilation);
        }
    }
}