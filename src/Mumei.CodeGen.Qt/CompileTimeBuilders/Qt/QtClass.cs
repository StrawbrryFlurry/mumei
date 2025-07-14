using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Playground.Qt;
using Mumei.CodeGen.Playground.Roslyn;
using Mumei.CodeGen.Qt.Output;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Qt.Qt;

public interface IQtParameter : IQtType {
    public int Modifiers { get; }
}

public interface IQtInvokable<TReturn> : IQtTemplateBindable;

public interface IQtTemplateBindable : ISyntaxRepresentable;

public interface IQtCompileTimeValue<out TValue> {
    public TActual As<TActual>();
};

public interface IQtThis : IQtCompileTimeValue<Arg.TThis>;

public interface IQtTypeDeclaration;

internal readonly struct QtDeclarationPtr<T>(
    List<T> declarationRef,
    int declarationIdx
) {
    public void Update(T newDeclaration) {
        declarationRef[declarationIdx] = newDeclaration;
    }
}

public readonly struct QtClass(
    AccessModifier modifiers,
    string name,
    QtTypeParameter[]? typeParameters = null!
) : IQtType, IQtTypeDeclaration {
    public QtTypeParameterList TypeParameters { get; } = new(typeParameters);

    private readonly List<QtFieldCore> _fields = new();
    private readonly List<QtMethodCore> _methods = new();

    public QtField<CompileTimeUnknown> AddField(
        AccessModifier modifiers,
        IQtType type,
        string name
    ) {
        return default!;
    }

    public QtField<TField> AddField<TField>(
        AccessModifier modifiers,
        string name
    ) {
        var field = new QtField<TField>(modifiers, name);
        _fields.Add(field.Field);
        return field;
    }

    public QtField<CompileTimeUnknown> Field(string name) {
        return default!;
    }

    public QtField<CompileTimeUnknown> AddField(
        AccessModifier modifiers,
        IQtType type,
        string name,
        IQtCompileTimeValue<CompileTimeUnknown> defaultValue
    ) {
        return default!;
    }

    public QtProperty<T> AddProperty<T>(
        AccessModifier modifiers,
        string name
    ) {
        return default!;
    }

    public QtMethod<Unit> AddMethod<TBindingCtx>(
        AccessModifier modifiers,
        string name,
        QtTypeParameter[] typeParameters,
        IQtType[] parameters,
        Action<IQtThis, TBindingCtx, QtMethodLegacy.QtMethodBuilderCtx> implementation,
        TBindingCtx bindingCtx
    ) {
        return default!;
    }

    public QtMethod<CompileTimeUnknown> AddMethod<TBindingCtx>(
        AccessModifier modifiers,
        IQtType returnType,
        string name,
        QtTypeParameter[] typeParameters,
        IQtType[] parameters, // Needs support for IQtParameter (out parammeters)
        Func<IQtThis, TBindingCtx, QtMethodLegacy.QtMethodBuilderCtx, object> implementation,
        TBindingCtx bindingCtx
    ) {
        return default!;
    }

    public QtMethod<TReturnType> AddMethod<TReturnType>(
        AccessModifier modifiers,
        string name,
        IQtType[] parameters,
        Func<IQtThis, QtMethodLegacy.QtMethodBuilderCtx, TReturnType> implementation
    ) {
        return default!;
    }

    public QtMethod<CompileTimeUnknown> AddMethod(
        AccessModifier modifiers,
        string name,
        Delegate implementation
    ) {
        return default!;
    }

    public QtMethod<Unit> AddProxyMethod(
        AccessModifier modifiers,
        string name,
        Action<IQtThis, QtMethodLegacy.QtProxyMethodBuilderCtx> implementation
    ) {
        return default!;
    }

    public QtMethod<TReturnType> AddProxyMethod<TReturnType>(
        AccessModifier modifiers,
        string name,
        Func<IQtThis, QtMethodLegacy.QtProxyMethodBuilderCtx, TReturnType> implementation
    ) {
        return default!;
    }

    public QtMethod<CompileTimeUnknown> AddProxyMethod(
        AccessModifier modifiers,
        IQtType returnType,
        string name,
        Func<IQtThis, QtMethodLegacy.QtProxyMethodBuilderCtx, object> implementation
    ) {
        return default!;
    }

    public QtMethod<CompileTimeUnknown> BindTemplateMethod<TTemplate>(
        TTemplate template,
        Func<TTemplate, Delegate> methodSelector
    ) where TTemplate : QtClassTemplate<TTemplate> {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public QtMethod<CompileTimeUnknown> BindTemplateProxyMethod<TTemplate>(
        TTemplate template
    ) where TTemplate : IQtInterceptorMethodTemplate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public QtMethod<CompileTimeUnknown> BindDynamicTemplateInterceptMethod(
        InvocationExpressionSyntax invocationToProxy,
        DeclareQtInterceptorMethod declaration
    ) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public QtMethod<CompileTimeUnknown> __BindDynamicTemplateInterceptMethod(
        InvocationExpressionSyntax invocationToProxy,
        __DynamicallyBoundSourceCode code
    ) {
        var decl = new QtDeclarationPtr<QtMethodCore>(_methods, _methods.Count + 1); // This isn't thread-safe, prolly doesn't matter though
        var factory = new RoslynQtMethodFactory();
        var method = factory.CreateProxyMethodForInvocation(
            invocationToProxy,
            new __DynamicallyBoundSourceCode(),
            decl
        );

        _methods.Add(method.Method);
        return method;
    }

    public QtMethod<CompileTimeUnknown> BindDynamicTemplateInterceptMethod<TTemplateReferences>(
        InvocationExpressionSyntax invocationToProxy,
        TTemplateReferences refs,
        DeclareQtInterceptorMethodWithRefs<TTemplateReferences> declaration
    ) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }


    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteFormatted(
            $$"""
              {{modifiers}} class{{TypeParameters}} {{name}} {
              """
        );

        writer.WriteLine();
        writer.Indent();

        foreach (var field in _fields) {
            field.WriteSyntax(writer);
            writer.WriteLine();
        }

        foreach (var method in _methods) {
            method.WriteSyntax(writer);
            writer.WriteLine();
        }

        writer.UnIndent();
        writer.WriteLine("}");
    }
}