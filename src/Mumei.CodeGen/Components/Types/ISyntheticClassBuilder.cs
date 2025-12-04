namespace Mumei.CodeGen.Components;

// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface ISyntheticClassBuilder<T> : ISimpleClassBuilder, ISyntheticClass, ISyntheticDeclaration, ISyntheticTypeInfo<T> {
    public IΦInternalClassBuilderCompilerApi ΦCompilerApi { get; }

    public ISyntheticClassBuilder<T> WithName(SyntheticIdentifier name);

    public ISyntheticClassBuilder<T> WithAccessibility(AccessModifierList accessModifiers);

    public ISyntheticClassBuilder<T> DeclareMethod(ISyntheticMethod method);

    public SyntheticIdentifier MakeUniqueName(string name);

    public ISyntheticMethodBuilder<Delegate> DeclareMethod<TMethodDefinition>(
        SyntheticIdentifier name,
        Func<TMethodDefinition, Delegate> methodSelector,
        Action<TMethodDefinition> inputBinder
    ) where TMethodDefinition : SyntheticMethodDefinition, new();

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TMethodDefinition, TSignature>(
        SyntheticIdentifier name,
        Func<TMethodDefinition, TSignature> methodSelector,
        Action<TMethodDefinition> inputBinder
    ) where TMethodDefinition : SyntheticMethodDefinition, new() where TSignature : Delegate;

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TSignature>(
        SyntheticIdentifier name
    ) where TSignature : Delegate;

    // public void BindSyntheticImplementation(ISyntheticType member, ISyntheticType actualType);

    public ISyntheticClassBuilder<T> WithTypeParameters(ISyntheticTypeParameterList typeParameters);

    public ISyntheticField<TField> DeclareField<TField>(ISyntheticField<TField> field);
    public ISyntheticField<TField> DeclareField<TField>(ISyntheticType type, SyntheticIdentifier name);

    public ISyntheticProperty<TProperty> DeclareProperty<TProperty>(ISyntheticProperty<TProperty> property);
    public ISyntheticProperty<TProperty> DeclareProperty<TProperty>(ISyntheticType type, SyntheticIdentifier name, SyntheticPropertyAccessorList accessors);

    public void DeclareConstructor<TImplementaiton>(Delegate impl);

    public void Implement(Type baseType);
    public void Implement(ISyntheticType baseType);

    public void Extend(Type baseType);
    public void Extend(ISyntheticType baseType);

    public ISyntheticClassBuilder<TNested> DeclareNestedClass<TNested>(SyntheticIdentifier name, Action<TNested> bindInputs) where TNested : ISyntheticClass;
}

// ReSharper disable once InconsistentNaming
public interface IΦInternalClassBuilderCompilerApi {
    public ICodeGenerationContext Context { get; }

    public ISyntheticType DynamicallyBoundType(string type);

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