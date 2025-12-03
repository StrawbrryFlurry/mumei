using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.Roslyn.Testing;

namespace Mumei.CodeGen.DeclarationGenerator.Tests;

public sealed class ClassDeclarationDefinitionGeneratorTests {
    [Fact]
    public void Test1() {
        SourceGeneratorTest.TestGenerator<ClassDeclarationDefinitionGenerator>(
            SyntaxTreeReference.Of(typeof(TestScope))
        ).RunWithAssert(result => { });
    }
}

file static class TestScope {
    public sealed partial class TestClassDefinition : SyntheticClassDefinition<TestClassDefinition> {
        [Input]
        public string InputA { get; }

        [Input]
        public ITypeSymbol InputB { get; }

        [Output]
        public string OutputA { get; }

        [Output]
        public Type OutputB { get; }

        public override void Setup(ISyntheticClassBuilder<TestClassDefinition> classBuilder) {
            // classBuilder.WithBaseClass(InputB);
        }

        [Output]
        public Task DoWorkAsync() {
            Console.WriteLine("Doing work...");
            return Task.CompletedTask;
        }
    }
}