namespace Mumei.CodeGen.Components;

public interface ISimpleSyntheticClassBuilder : ISyntheticDeclaration {
    public IΦInternalClassBuilderCompilerApi ΦCompilerApi { get; }

    public SyntheticIdentifier MakeUniqueName(string name);

    public TMethod DeclareMethod<TMethod>(TMethod method) where TMethod : ISyntheticMethod;
    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TSignature>(SyntheticIdentifier name) where TSignature : Delegate;

    public TField DeclareField<TField>(TField field) where TField : ISyntheticField;
    public ISyntheticFieldBuilder<TField> DeclareField<TField>(ISyntheticType type, SyntheticIdentifier name);

    public TProperty DeclareProperty<TProperty>(TProperty property) where TProperty : ISyntheticProperty;
    public ISyntheticPropertyBuilder<TProperty> DeclareProperty<TProperty>(ISyntheticType type, SyntheticIdentifier name, SyntheticPropertyAccessorList accessors);

    public void DeclareConstructor<TImplementaiton>(Delegate impl);

    public void Implement(Type baseType);
    public void Implement(ISyntheticType baseType);

    public void Extend(Type baseType);
    public void Extend(ISyntheticType baseType);

    public ISyntheticClassBuilder<TNested> DeclareNestedClass<TNested>(SyntheticIdentifier name);
}