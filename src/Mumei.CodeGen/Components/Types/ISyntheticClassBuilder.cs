namespace Mumei.CodeGen.Components;

// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface ISyntheticClassBuilder<T> : ISimpleSyntheticClassBuilder, ISyntheticClass, ISyntheticTypeInfo<T> {
    public IΦInternalClassBuilderCompilerApi ΦCompilerApi { get; }

    public ISyntheticClassBuilder<T> WithName(SyntheticIdentifier name);

    public ISyntheticClassBuilder<T> WithAccessibility(AccessModifierList accessModifiers);

    public ISyntheticClassBuilder<T> WithDeclaration(ISyntheticDeclaration declaration);

    public ISyntheticMethodBuilder<Delegate> DeclareMethod<TMethodDefinition>(
        SyntheticIdentifier name,
        Func<TMethodDefinition, Delegate> methodSelector,
        Action<TMethodDefinition> inputBinder
    ) where TMethodDefinition : SyntheticMethodDefinition, new();

    // public void BindSyntheticImplementation(ISyntheticType member, ISyntheticType actualType);

    public ISyntheticClassBuilder<T> WithTypeParameters(ISyntheticTypeParameterList typeParameters);
}

// ReSharper disable once InconsistentNaming
public interface IΦInternalClassBuilderCompilerApi {
    public ICodeGenerationContext Context { get; }

    public void DeclareMethod(
        ISyntheticAttribute[] attributes,
        AccessModifierList modifiers,
        ISyntheticType returnType,
        SyntheticIdentifier name,
        ISyntheticTypeParameter[] typeParameters,
        ISyntheticParameter[] parameters,
        ISyntheticCodeBlock body
    );
}