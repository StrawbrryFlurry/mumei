using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

/// <summary>
/// Declares that a synthetic component can be constructed into a target type.
/// TODO: Consider if we require components such as methods to implement this for their target type (e.g. MethodDeclaration)
/// or if this should be implicit to not expose this requirement to components that it doesn't relate to
/// (e.g. needing to implement Construct for non-applicable types that still need to participate in the synthetic compilation).
/// </summary>
/// <typeparam name="TTarget"></typeparam>
internal interface ISyntheticConstructable<out TTarget> {
    public TTarget Construct(ICompilationUnitContext compilationUnit);
}

internal interface ICompilationUnitContext {
    public ICodeGenerationContext CodeGenContext { get; }

    public void AddSharedLocalDeclaration(ICompilationUnitFeature feature);

    public T Synthesize<T>(object? constructable, T? defaultValue = default);
    public T? SynthesizeOptional<T>(object? constructable);
}

internal interface ICompilationUnitFeature {
    public CompilationUnitFragment Implement(ICompilationUnitContext compilationUnit, CompilationUnitFragment currentUnit);
}