using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.CodeGen.Roslyn.RoslynCodeProviders;
using Mumei.Roslyn.Testing;

namespace Mumei.CodeGen.DeclarationGenerator.IntegrationTests;

public sealed class ClassDeclarationDefinitionGeneratorTests {
    [Fact]
    public void Test1() {
        SourceGeneratorTest.MetaGeneratorTest(
            SyntaxTreeReference.Of(typeof(TestScope)),
            context => {
                var x = context.CreateQtProvider(
                    (a, b) => true,
                    (a, b) => { return ""; }
                );

                var o = x.IncrementalGenerate((generationContext, s) => {
                    generationContext.DeclareClass<TestClassDefinition<CompileTimeUnknown>>(
                        "",
                        (cls) => { });
                });

                context.RegisterCodeGenerationOutput(o);
            }
        ).RunWithAssert(result => {
            result.HasFileMatching($"A__0.g.cs").WithContent($"");
        });
    }
}

public sealed partial class TestClassDefinition<TState> : SyntheticClassDefinition<TestClassDefinition<TState>> {
    [Input]
    public string InputA { get; }

    [Input]
    public ITypeSymbol InputB { get; }

    [Output]
    public string OutputA { get; }

    [Output]
    public Type OutputB { get; }

    [Output]
    private TState _state;

    public override void Setup(ISyntheticClassBuilder<TestClassDefinition<TState>> classBuilder) {
        classBuilder.Bind(typeof(TState), InputB);
        // classBuilder.WithBaseClass(InputB);
    }

    [Output]
    public Task DoWorkAsync(TState state) {
        _state = state;
        Console.WriteLine("Doing work...");
        return Task.CompletedTask;
    }
}

file static class TestScope { }