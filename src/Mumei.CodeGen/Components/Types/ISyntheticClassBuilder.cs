using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Components;

public interface ISyntheticClassBuilder<T> : ISyntheticClass, ISyntheticTypeInfo<T> {
    public IΦInternalClassBuilderCompilerApi ΦCompilerApi { get; }

    public ISyntheticClassBuilder<T> WithName(string name);

    public ISyntheticClassBuilder<T> WithAccessibility(AccessModifierList accessModifiers);

    public ISyntheticClassBuilder<T> DeclareMethod(ISyntheticMethod method);

    public string UniqueName(string name);

    public ISyntheticMethodBuilder<Delegate> DeclareMethod<TMethodDefinition>(
        string name,
        Func<TMethodDefinition, Delegate> methodSelector,
        Action<TMethodDefinition> inputBinder
    ) where TMethodDefinition : SyntheticMethodDefinition, new();

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TMethodDefinition, TSignature>(
        string name,
        Func<TMethodDefinition, TSignature> methodSelector,
        Action<TMethodDefinition> inputBinder
    ) where TMethodDefinition : SyntheticMethodDefinition, new() where TSignature : Delegate;

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TSignature>(
        string name
    ) where TSignature : Delegate;

    public void BindSyntheticImplementation(ISyntheticType member, ISyntheticType actualType);
    public void BindSyntheticImplementation(Type member, ISyntheticType actualType);
    public void BindSyntheticImplementation(Type member, ITypeSymbol actualType);

    public ISyntheticField<TField> DeclareField<TField>(string name);

    public ISyntheticField<CompileTimeUnknown> DeclareField(Type type, string name);
    public ISyntheticField<CompileTimeUnknown> DeclareField(ITypeSymbol type, string name);
    public ISyntheticField<CompileTimeUnknown> DeclareField(ISyntheticType type, string name);

    void DeclareProperty(ITypeSymbol type, string name);

    public ISyntheticClassBuilder<T> Bind<TTarget>(ITypeSymbol type);

    public void DeclareConstructor<TImplementaiton>(Delegate impl);


    public void Implement(Type baseType);
    public void Implement(ISyntheticType baseType);
    public void Implement(ITypeSymbol baseTyp);

    public void Extend(Type baseType);
    public void Extend(ISyntheticType baseType);
    public void Extend(ITypeSymbol baseType);

    public ISyntheticClassBuilder<TNested> DeclareNestedClass<TNested>(string name, Action<TNested> bindInputs) where TNested : ISyntheticClass;
}

// ReSharper disable once InconsistentNaming
public interface IΦInternalClassBuilderCompilerApi {
    public ICodeGenerationContext Context { get; }

    public void DeclareMethod(
        ISyntheticAttribute[] attributes,
        AccessModifierList modifiers,
        ISyntheticType returnType,
        string name,
        ISyntheticTypeParameter[] typeParameters,
        ISyntheticParameter[] parameters,
        ISyntheticCodeBlock body
    );
}