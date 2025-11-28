using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing;

public static class SourceGeneratorTest {
    public static IncrementalSourceGeneratorTest<TGenerator> TestGenerator<TGenerator>(Action<TestCompilationBuilder> setupCompilation)
        where TGenerator : IIncrementalGenerator, new() {
        var builder = new TestCompilationBuilder();
        setupCompilation(builder);
        var compilation = builder.Build();

        return new IncrementalSourceGeneratorTest<TGenerator>(compilation);
    }
}