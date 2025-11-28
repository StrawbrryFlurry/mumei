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

internal interface ICompilationUnitContext : IComponentSynthesizer {
    public ICodeGenerationContext CodeGenContext { get; }
}