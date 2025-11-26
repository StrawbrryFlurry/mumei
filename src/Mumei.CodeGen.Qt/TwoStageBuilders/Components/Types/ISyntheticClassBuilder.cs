using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticClassBuilder<T> : ISyntheticClass, ISyntheticTypeInfo<T> {
    public IλInternalClassBuilderCompilerApi λCompilerApi { get; }

    public ISyntheticClassBuilder<T> WithName(string name);

    public ISyntheticClassBuilder<T> WithModifiers(AccessModifierList modifiers);

    public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod<TMethodDefinition>(
        string name,
        InvocationExpressionSyntax invocationToIntercept,
        Func<TMethodDefinition, Delegate> methodSelector,
        Action<TMethodDefinition>? inputBinder = null
    ) where TMethodDefinition : SyntheticInterceptorMethodDefinition, new();

    public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod(
        string name,
        InvocationExpressionSyntax invocationToIntercept
    );

    /// <summary>
    /// Same as <see cref="DeclareInterceptorMethod"/>, but does not bind the declaration
    /// to the intercepted invocation's type arguments.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="invocationToIntercept"></param>
    /// <returns></returns>
    public ISyntheticInterceptorMethodBuilder<Delegate> DeclareUnconstructedInterceptorMethod(
        string name,
        InvocationExpressionSyntax invocationToIntercept
    );

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

    public string MakeUniqueName(string n);

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
public interface IλInternalClassBuilderCompilerApi {
    public ISyntheticCompilation Compilation { get; }

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