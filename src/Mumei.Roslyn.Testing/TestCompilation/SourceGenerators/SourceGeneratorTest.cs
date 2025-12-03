using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing;

public static class SourceGeneratorTest {
    public static IncrementalSourceGeneratorTest<TGenerator> TestGenerator<TGenerator>(Action<TestCompilationBuilder> setupCompilation)
        where TGenerator : IIncrementalGenerator, new() {
        var builder = new TestCompilationBuilder();
        setupCompilation(builder);
        var compilation = builder.Build();

        return new IncrementalSourceGeneratorTest<TGenerator>(compilation, new TGenerator());
    }

    public static IncrementalSourceGeneratorTest<TGenerator> TestGenerator<TGenerator>(
        ICompilationReference compilationReference
    ) where TGenerator : IIncrementalGenerator, new() {
        return TestGenerator<TGenerator>(b => b.AddReference(compilationReference));
    }

    public static IncrementalSourceGeneratorTest<MetaTestGenerator> MetaGeneratorTest(
        ICompilationReference testCompilationReference,
        Action<IncrementalGeneratorInitializationContext> setup
    ) {
        var builder = new TestCompilationBuilder();
        builder.AddReference(testCompilationReference);
        var compilation = builder.Build();

        return new IncrementalSourceGeneratorTest<MetaTestGenerator>(
            compilation,
            new MetaTestGenerator(setup)
        );
    }
}

public sealed class MetaTestGenerator(Action<IncrementalGeneratorInitializationContext> setup) : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        setup(context);
    }
}