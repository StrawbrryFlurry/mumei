using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Roslyn;

namespace Mumei.CodeGen.Qt.Qt;

public interface IQtParameter : IQtType {
    public int Modifiers { get; }
}

public interface IQtInvokable<TReturn> : IQtTemplateBindable;

public interface IQtThis {
    public T Is<T>();
}

public interface IQtTypeDeclaration : IQtTemplateBindable, IRenderNode;

internal readonly struct QtDeclarationPtr<T>(
    List<T> declarationRef,
    int declarationIdx
) {
    public void Update(T newDeclaration) {
        if (declarationRef is null) { throw new InvalidOperationException("The declaration is not referenced by any container that can be updated"); }

        declarationRef[declarationIdx] = newDeclaration;
    }
}

public readonly struct ConstructedGenericQtClass(
    QtClass classRef,
    QtCollection<IQtType> typeArguments
) : IQtType, IQtTypeDeclaration {
    public void RenderFullName(IRenderer renderer) { }

    public void RenderExpression(IRenderer renderer) { }

    public void RenderFullName(IRenderTreeBuilder renderer) {
        renderer.Text(classRef.Name);
        renderer.Text("<");
        renderer.SeparatedList(typeArguments.Span, static x => x.FullName);
        renderer.Text(">");
    }

    public void RenderExpression(IRenderTreeBuilder renderer) {
        renderer.Text(classRef.Name);
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        throw new NotImplementedException();
    }

    public void Render(IRenderTreeBuilder renderTree) {
        throw new NotImplementedException();
    }
}

public class QtClass(
    AccessModifier modifiers,
    string name,
    QtCollection<QtTypeParameter> typeParameters = default,
    IQtTypeDeclaration? parentClassOrNamespace = null
) : IQtType, IQtTypeDeclaration, IDebugRenderNodeFormattable {
    public QtTypeParameterList TypeParameters { get; } = new(typeParameters);

    private readonly List<QtFieldCore> _fields = new();
    private readonly List<QtMethodRenderNode> _methods = new();

    public string Name => name;

    public static QtClass CreateObfuscated(
        AccessModifier modifiers,
        string nameHint,
        QtCollection<QtTypeParameter> typeParameters = default
    ) {
        var name = RandomNameGenerator.GenerateName(nameHint);
        return new QtClass(modifiers, name, typeParameters);
    }

    // All other implementations should be extension methods
    public void AddField<T>(
        ref QtField<T> field
    ) {
        _fields.Add(field.Field);
    }

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
        IQtCompileTimeValue defaultValue
    ) {
        return default!;
    }

    public QtProperty<T> AddProperty<T>(
        AccessModifier modifiers,
        string name
    ) {
        return default!;
    }

    public QtMethod<T> AddMethod<T>(ref QtMethod<T> method) {
        _methods.Add(method.Method);
        return method;
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

    public QtMethod<CompileTimeUnknown> AddTemplateMethod<TTemplate>(
        TTemplate template,
        Func<TTemplate, Delegate> methodSelector
    ) where TTemplate : QtClassTemplate<TTemplate> {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public QtMethod<CompileTimeUnknown> AddDynamicTemplateInterceptMethod<TReturn>(
        InvocationExpressionSyntax invocationToProxy,
        DeclareQtInterceptorMethod<TReturn> declaration
    ) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public QtMethod<CompileTimeUnknown> AddDynamicTemplateInterceptMethod(
        InvocationExpressionSyntax invocationToProxy,
        DeclareQtInterceptorVoidMethod declaration
    ) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public QtMethod<CompileTimeUnknown> __BindDynamicTemplateInterceptMethod(
        InvocationExpressionSyntax invocationToProxy,
        __DynamicallyBoundSourceCode code
    ) {
        var decl = new QtDeclarationPtr<QtMethodRenderNode>(_methods, _methods.Count + 1); // This isn't thread-safe, prolly doesn't matter though
        var factory = new RoslynQtMethodFactory(QtCompilationScope.Active);
        var method = factory.CreateProxyMethodForInvocation(
            invocationToProxy,
            code,
            decl
        );

        _methods.Add(method.Method);
        return method;
    }

    public QtMethod<TReturn> AddDynamicTemplateInterceptMethod<TTemplateReferences, TReturn>(
        InvocationExpressionSyntax invocationToProxy,
        TTemplateReferences refs,
        DeclareQtInterceptorMethodWithRefs<TTemplateReferences, TReturn> declaration
    ) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public QtMethod<CompileTimeUnknown> AddDynamicTemplateInterceptMethod<TTemplateReferences>(
        InvocationExpressionSyntax invocationToProxy,
        TTemplateReferences refs,
        DeclareQtInterceptorVoidMethodWithRefs<TTemplateReferences> declaration
    ) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public QtMethod<CompileTimeUnknown> __BindDynamicTemplateInterceptMethod(
        InvocationExpressionSyntax invocationToProxy,
        __DynamicallyBoundSourceCode code,
        QtDynamicComponentBinderCollection? dynamicComponentBinders
    ) {
        var decl = new QtDeclarationPtr<QtMethodRenderNode>(_methods, _methods.Count + 1); // This isn't thread-safe, prolly doesn't matter though
        var factory = new RoslynQtMethodFactory(QtCompilationScope.Active);
        var method = factory.CreateProxyMethodForInvocation(
            invocationToProxy,
            code,
            decl,
            dynamicComponentBinders
        );

        _methods.Add(method.Method);
        return method;
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteFormatted(
            $$"""
              {{modifiers}} class{{TypeParameters}} {{name}} {
              """
        );

        writer.WriteLine();
        writer.Indent();

        foreach (var field in _fields) {
            field.WriteSyntax(ref writer);
            writer.WriteLine();
        }

        foreach (var method in _methods) {
            method.WriteSyntax(ref writer);
            writer.WriteLine();
        }

        writer.Dedent();
        writer.WriteLine("}");
    }

    public void RenderFullName(IRenderTreeBuilder renderer) {
        // Render parent node
        renderer.Text(name);
    }

    public void RenderExpression(IRenderTreeBuilder renderer) {
        renderer.Text(name);
    }

    public ConstructedGenericQtClass Construct(QtCollection<IQtType> typeArguments) {
        if (typeArguments.Count != TypeParameters.Count) {
            throw new ArgumentException($"Type argument count ({typeArguments.Count}) does not match type parameter count ({TypeParameters.Count})");
        }

        return new ConstructedGenericQtClass(this, typeArguments);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate(
            $$"""
              {{modifiers.List}} class{{TypeParameters}} {{name}}
              """
        );
        renderTree.Text(" ");
        renderTree.StartCodeBlock();

        foreach (var field in _fields) {
            // field.WriteSyntax(ref writer);
            // writer.WriteLine();
        }

        for (var i = 0; i < _methods.Count; i++) {
            var method = _methods[i];
            renderTree.Node(method);
            if (i < _methods.Count - 1) {
                renderTree.NewLine();
            }
        }

        renderTree.EndCodeBlock();
    }

    public string DescribeDebugNode() {
        return $"QtClass {{ Name = {name} }}";
    }
}

public static class QtClassDynamicDeclarationExtensions {
    public static QtMethod<CompileTimeUnknown> AddTemplateInterceptMethod<TTemplate>(
        this QtClass cls,
        InvocationExpressionSyntax invocationExpression,
        TTemplate template,
        Func<TTemplate, Delegate> methodSelector
    ) where TTemplate : IQtInterceptorMethodTemplate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    internal static AddTemplateInterceptMethodArguments ParseAddTemplateInterceptMethodArguments(
        IInvocationOperation invocation
    ) {
        var arguments = invocation.Arguments;
        return new AddTemplateInterceptMethodArguments(
            arguments[0],
            arguments[1],
            arguments[2],
            arguments[3]
        );
    }

    internal readonly record struct AddTemplateInterceptMethodArguments(
        IArgumentOperation ClassRef,
        IArgumentOperation InvocationToProxy,
        IArgumentOperation Template,
        IArgumentOperation MethodSelector
    );
}