using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Qt.Tests.Setup;

internal sealed class IncrementalSourceGeneratorTest<TGenerator> where TGenerator : IIncrementalGenerator, new() {
    private readonly TGenerator _generator = new();

    public Result Run() {
        return null!;
    }

    public void UpdateCompilation() {}

    public sealed class Result { }
}