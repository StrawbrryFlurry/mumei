﻿using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            (CSharpParseOptions)_compilation.SyntaxTrees.First().Options
        );

        var runResult = driver.RunGeneratorsAndUpdateCompilation(
            _compilation,
            out var updatedCompilation,
            out var diagnostics
        ).GetRunResult();

        Assert.Empty(diagnostics);
        Assert.Empty(runResult.Diagnostics);
        Assert.True(runResult.GeneratedTrees.Length > 0);

        var compilationDiagnostics = updatedCompilation.GetDiagnostics();

        if (compilationDiagnostics.Any()) {

            Assert.Multiple(compilationDiagnostics.Select<Diagnostic, Action>(diagnostic => {
                    var source = diagnostic.Location.SourceTree;
                    return () => Assert.Fail(
                        $"Diagnostic: {diagnostic.Id}:\n{diagnostic.GetMessage()}\nat: {source.GetText().GetSubText(diagnostic.Location.SourceSpan)}\nof: {source.GetText()}"
                    );
                }
            ).ToArray());
        }

        return new SourceGeneratorTestResult(runResult, updatedCompilation);
    }
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(SourceGeneratorTestDebugView))]
public sealed class SourceGeneratorTestResult(
    GeneratorDriverRunResult runResult,
    Compilation compilation
) {
    public ImmutableArray<SyntaxTree> GeneratedTrees => runResult.GeneratedTrees;
    public string DebuggerDisplay => $"{GeneratedTrees.Length} Generated Trees";

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