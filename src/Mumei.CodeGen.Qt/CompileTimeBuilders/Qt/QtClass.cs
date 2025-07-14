using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Playground.Qt;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public interface IQtParameter : IQtType {
    public int Modifiers { get; }
}

public interface IQtInvokable<TReturn> : IQtTemplateBindable;

public interface IQtTemplateBindable;

public interface IQtCompileTimeValue<out TValue> {
    public TActual As<TActual>();
};

public interface IQtThis : IQtCompileTimeValue<Arg.TThis>;

public sealed class QtClass(
    AccessModifier modifiers,
    string name,
    QtTypeParameter[]? typeParameters = null!
) : IQtType {
    public QtTypeParameterCollection TypeParameters { get; } = new(typeParameters);

    private readonly List<IQtComponent> _fields = new();

    public QtField<CompileTimeUnknown> AddField(
        AccessModifier modifiers,
        IQtType type,
        string name
    ) {
        return null!;
    }

    public QtField<TField> AddField<TField>(
        AccessModifier modifiers,
        string name
    ) {
        var field = new QtField<TField>(modifiers, name);
        _fields.Add(field);
        return field;
    }

    public QtField<CompileTimeUnknown> Field(string name) {
        return null!;
    }

    public QtField<CompileTimeUnknown> AddField(
        AccessModifier modifiers,
        IQtType type,
        string name,
        IQtCompileTimeValue<CompileTimeUnknown> defaultValue
    ) {
        return null!;
    }

    public QtProperty<T> AddProperty<T>(
        AccessModifier modifiers,
        string name
    ) {
        return null!;
    }

    public QtMethod<Unit> AddMethod<TBindingCtx>(
        AccessModifier modifiers,
        string name,
        QtTypeParameter[] typeParameters,
        IQtType[] parameters,
        Action<IQtThis, TBindingCtx, QtMethod.QtMethodBuilderCtx> implementation,
        TBindingCtx bindingCtx
    ) {
        return null!;
    }

    public QtMethod<CompileTimeUnknown> AddMethod<TBindingCtx>(
        AccessModifier modifiers,
        IQtType returnType,
        string name,
        QtTypeParameter[] typeParameters,
        IQtType[] parameters, // Needs support for IQtParameter (out parammeters)
        Func<IQtThis, TBindingCtx, QtMethod.QtMethodBuilderCtx, object> implementation,
        TBindingCtx bindingCtx
    ) {
        return null!;
    }

    public QtMethod<TReturnType> AddMethod<TReturnType>(
        AccessModifier modifiers,
        string name,
        IQtType[] parameters,
        Func<IQtThis, QtMethod.QtMethodBuilderCtx, TReturnType> implementation
    ) {
        return null!;
    }

    public QtMethod<CompileTimeUnknown> AddMethod(
        AccessModifier modifiers,
        string name,
        Delegate implementation
    ) {
        return null!;
    }

    public QtMethod<Unit> AddProxyMethod(
        AccessModifier modifiers,
        string name,
        Action<IQtThis, QtMethod.QtProxyMethodBuilderCtx> implementation
    ) {
        return null!;
    }

    public QtMethod<TReturnType> AddProxyMethod<TReturnType>(
        AccessModifier modifiers,
        string name,
        Func<IQtThis, QtMethod.QtProxyMethodBuilderCtx, TReturnType> implementation
    ) {
        return null!;
    }

    public QtMethod<CompileTimeUnknown> AddProxyMethod(
        AccessModifier modifiers,
        IQtType returnType,
        string name,
        Func<IQtThis, QtMethod.QtProxyMethodBuilderCtx, object> implementation
    ) {
        return null!;
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

    public QtMethod<CompileTimeUnknown> BindDynamicTemplateProxyMethod<TTemplate>(
        TTemplate template
    ) where TTemplate : IQtDynamicInterceptorMethodTemplate {
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

        writer.UnIndent();
        writer.WriteLine("}");
    }
}

public sealed class Unit;

public sealed class CompileTimeUnknown;