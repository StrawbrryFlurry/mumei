using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen;

/// <summary>
/// Base class for Mumei generators. Annotate the derived class with <see cref="GeneratorAttribute"/>:
/// <code>
/// [Generator]
/// public sealed class MyGenerator : MumeiGenerator {
///     protected override void Initialize(GeneratorContext gen) {
///         gen.TypesWith("MyLib.ReprAttribute").Generate((type, output) => ...);
///     }
/// }
/// </code>
/// </summary>
public abstract class MumeiGenerator : IIncrementalGenerator {
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context) {
        Initialize(new GeneratorContext(context));
    }

    protected abstract void Initialize(GeneratorContext gen);
}
