using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Components;

public interface ISimpleClassBuilder : ISyntheticDeclaration {
    public IΦInternalClassBuilderCompilerApi ΦCompilerApi { get; }

    public void Bind(Type t, ISyntheticType actualType, [CallerArgumentExpression(nameof(t))] string bindingTargetExpression = "");

    public SyntheticIdentifier MakeUniqueName(string name);

    public TMethod DeclareMethod<TMethod>(TMethod method) where TMethod : ISyntheticMethod;

    public ISyntheticField<TField> DeclareField<TField>(ISyntheticField<TField> field);
    public ISyntheticField<TField> DeclareField<TField>(ISyntheticType type, SyntheticIdentifier name);

    public TProperty DeclareProperty<TProperty>(TProperty property) where TProperty : ISyntheticProperty;
    public ISyntheticPropertyBuilder<TProperty> DeclareProperty<TProperty>(ISyntheticType type, SyntheticIdentifier name, SyntheticPropertyAccessorList accessors);

    public void DeclareConstructor<TImplementaiton>(Delegate impl);

    public void Implement(Type baseType);
    public void Implement(ISyntheticType baseType);

    public void Extend(Type baseType);
    public void Extend(ISyntheticType baseType);

    public ISyntheticClassBuilder<TNested> DeclareNestedClass<TNested>(SyntheticIdentifier name);
}