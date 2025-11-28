using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Components;

internal sealed partial class QtSyntheticClassBuilder<TClassDef> {
    public ISyntheticMethodBuilder<Delegate> DeclareMethod<TMethodDefinition>(
        string name,
        Func<TMethodDefinition, Delegate> methodSelector,
        Action<TMethodDefinition> inputBinder
    ) where TMethodDefinition : SyntheticMethodDefinition, new() {
        throw new NotImplementedException();
    }

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TMethodDefinition, TSignature>(
        string name,
        Func<TMethodDefinition, TSignature> methodSelector,
        Action<TMethodDefinition> inputBinder
    ) where TMethodDefinition : SyntheticMethodDefinition, new() where TSignature : Delegate {
        throw new NotImplementedException();
    }

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TSignature>(string name) where TSignature : Delegate {
        throw new NotImplementedException();
    }

    public void BindSyntheticImplementation(ISyntheticType member, ISyntheticType actualType) {
        throw new NotImplementedException();
    }

    public void BindSyntheticImplementation(Type member, ISyntheticType actualType) {
        throw new NotImplementedException();
    }

    public void BindSyntheticImplementation(Type member, ITypeSymbol actualType) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TNew> WithDefinition<TNew>() where TNew : ISyntheticClass {
        throw new NotImplementedException();
    }

    public ISyntheticField<TField> DeclareField<TField>(string name) {
        throw new NotImplementedException();
    }

    public ISyntheticField<CompileTimeUnknown> DeclareField(Type type, string name) {
        throw new NotImplementedException();
    }

    public ISyntheticField<CompileTimeUnknown> DeclareField(ITypeSymbol type, string name) {
        throw new NotImplementedException();
    }

    public ISyntheticField<CompileTimeUnknown> DeclareField(ISyntheticType type, string name) {
        throw new NotImplementedException();
    }

    public void DeclareProperty(ITypeSymbol type, string name) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TClassDef> Bind<TTarget>(ITypeSymbol type) {
        throw new NotImplementedException();
    }

    public void DeclareConstructor<TImplementaiton>(Delegate impl) {
        throw new NotImplementedException();
    }

    public void Implement(Type baseType) {
        throw new NotImplementedException();
    }

    public void Implement(ISyntheticType baseType) {
        throw new NotImplementedException();
    }

    public void Implement(ITypeSymbol baseTyp) {
        throw new NotImplementedException();
    }

    public void Extend(Type baseType) {
        throw new NotImplementedException();
    }

    public void Extend(ISyntheticType baseType) {
        throw new NotImplementedException();
    }

    public void Extend(ITypeSymbol baseType) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TNested> DeclareNestedClass<TNested>(string name, Action<TNested> bindInputs) where TNested : ISyntheticClass {
        throw new NotImplementedException();
    }
}