using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;
using Mumei.Roslyn.Reflection;

namespace Mumei.CodeGen.Qt.Roslyn;

// ReSharper disable once InconsistentNaming
public readonly ref struct __DynamicallyBoundSourceCode {
    internal const string DynamicallyBoundSourceCodeStart = "<>SM:";
    internal const string DynamicallyBoundSourceCodeEnd = "ɵ";

    public required string[] CodeTemplate { get; init; }
}

file readonly ref struct DynamicSourceCodeBinder(
    in __DynamicallyBoundSourceCode code,
    ISourceCodeBinder binder
) : IQtTemplateBindable {
    private readonly string[] _template = code.CodeTemplate;

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        var t = _template;
        foreach (var section in t) {
            if (section.StartsWith(__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeStart)) {
                binder.WriteBindableSyntax(ref writer, section[__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeStart.Length..]);
                continue;
            }

            writer.Write(section);
        }
    }
}

internal interface ISourceCodeBinder {
    public void WriteBindableSyntax<TSyntaxWriter>(
        ref TSyntaxWriter writer,
        in ReadOnlySpan<char> key
    ) where TSyntaxWriter : ISyntaxWriter;
}

internal sealed class InvocationExpressionSourceCodeBinder(
    QtCompilationScope scope,
    InvocationExpressionSyntax invocation,
    QtMethodCore? qtMethod
) : ISourceCodeBinder {
    public void WriteBindableSyntax<TSyntaxWriter>(
        ref TSyntaxWriter writer,
        in ReadOnlySpan<char> key
    ) where TSyntaxWriter : ISyntaxWriter {
        var actualKey = key;
        if (actualKey is "Invoke") {
            WriteInvocation(ref writer);
            return;
        }
    }

    private void WriteInvocation<TSyntaxWriter>(ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        var method = scope.GetMethodSymbol(invocation);

        if (method.IsStatic | method.IsExtensionMethod) {
            writer.Write(method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }
        else {
            writer.Write("self");
        }

        writer.Write($".{method.Name}(");
    }
}

internal readonly ref struct RoslynQtMethodFactory(
    QtCompilationScope scope
) {
    public QtMethod<CompileTimeUnknown> CreateProxyMethodForInvocation(
        InvocationExpressionSyntax invocationToProxy,
        __DynamicallyBoundSourceCode sourceCode,
        QtDeclarationPtr<QtMethodCore> declPtr
    ) {
        var methodSymbol = scope.GetMethodSymbol(invocationToProxy);
        var invocationBinder = new InvocationExpressionSourceCodeBinder(scope, invocationToProxy, default);
        var binder = new DynamicSourceCodeBinder(sourceCode, invocationBinder);
        var syntaxWriter = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
        binder.WriteSyntax(ref syntaxWriter);

        var factory = new RoslynQtComponentFactory(scope);

        var method = new QtMethod<CompileTimeUnknown>(
            "QtProxy__" + methodSymbol.Name,
            AccessModifier.FileStatic,
            factory.Type(methodSymbol.ReturnType),
            new QtTypeParameterList(), // Since this is a proxy method, all types need to be bound to the types at the call site
            factory.ParametersOf(methodSymbol),
            new StaticQtMethodRepresentation(syntaxWriter.ToString()),
            new QtAttributeList(),
            declPtr
        );

        return method;
    }
}