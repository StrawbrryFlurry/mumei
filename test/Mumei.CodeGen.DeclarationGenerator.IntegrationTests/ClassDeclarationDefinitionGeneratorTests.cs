using System.Runtime.InteropServices.ComTypes;
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
                var firstNode = true;
                var x = context.CreateQtProvider(
                    (a, b) => { return firstNode || (firstNode = false); },
                    (a, b) => { return ""; }
                );

                var o = x.IncrementalGenerate((ctx, s) => {
                    var cls = ctx.DeclareClass<TestClassDefinition<CompileTimeUnknown>>(
                        SyntheticIdentifier.Unique(ctx.GlobalNamespace, "A"),
                        (cls) => {
                            cls.InputB = ctx.TypeFromCompilation(typeof(string));
                            cls.InputA = "Ello";
                        });

                    ctx.EmitIncremental("A", cls);
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
    public string InputA { get; set; }

    [Input]
    public ITypeSymbol InputB { get; set; }

    [Output]
    public string OutputA { get; set; }

    [Output]
    public Type OutputB { get; }

    [Output]
    private TState _state;

    public override void Setup(ISyntheticClassBuilder<TestClassDefinition<TState>> classBuilder) {
        this.Bind(typeof(TState), InputB);
        // classBuilder.WithBaseClass(InputB);
    }

    [Output]
    public Task DoWorkAsync(TState state) {
        _state = state;
        Console.WriteLine($"Doing work... {InputA}");
        return Task.CompletedTask;
    }
}

file static class TestScope { }