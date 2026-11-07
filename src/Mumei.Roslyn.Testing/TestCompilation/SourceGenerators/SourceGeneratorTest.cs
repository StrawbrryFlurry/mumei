using Microsoft.CodeAnalysis;
using Mumei.CodeGen;

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

    public static IncrementalSourceGeneratorTest<MumeiTestGenerator> MumeiGeneratorTest(
        ICompilationReference testCompilationReference,
        Action<GeneratorContext> setup
    ) {
        return MumeiGeneratorTest(b => b.AddReference(testCompilationReference), setup);
    }

    public static IncrementalSourceGeneratorTest<MumeiTestGenerator> MumeiGeneratorTest(
        Action<TestCompilationBuilder> setupCompilation,
        Action<GeneratorContext> setup
    ) {
        var builder = new TestCompilationBuilder();
        setupCompilation(builder);
        return new IncrementalSourceGeneratorTest<MumeiTestGenerator>(builder.Build(), new MumeiTestGenerator(setup));
    }
}

public sealed class MumeiTestGenerator(Action<GeneratorContext> setup) : MumeiGenerator {
    protected override void Initialize(GeneratorContext gen) {
        setup(gen);
    }
}

public sealed class MetaTestGenerator(Action<IncrementalGeneratorInitializationContext> setup) : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        setup(context);
    }
}