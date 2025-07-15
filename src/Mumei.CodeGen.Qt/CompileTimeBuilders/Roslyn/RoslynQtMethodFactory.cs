using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground.Roslyn;

// ReSharper disable once InconsistentNaming
public readonly ref struct __DynamicallyBoundSourceCode {
    internal const string DynamicallyBoundSourceCodeStart = "<>SM:";
    internal const string DynamicallyBoundSourceCodeEnd = "ɵ";

    public required string[] CodeTemplate { get; init; }
}

file readonly ref struct DynamicSourceCodeBinder(
    in __DynamicallyBoundSourceCode code,
    ISourceCodeBinder binder,
    ref readonly SourceCodeBinderCtx ctx
) : IQtTemplateBindable {
    private readonly string[] _template = code.CodeTemplate;
    private readonly SourceCodeBinderCtx _ctx = ctx;

    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        var t = _template;
        foreach (var section in t) {
            if (!section.StartsWith(__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeStart)) {
                writer.Write(section);
                continue;
            }

            binder.WriteBindableSyntax(writer, section, _ctx);
        }
    }
}

internal interface ISourceCodeBinder {
    public void WriteBindableSyntax<TSyntaxWriter>(
        in TSyntaxWriter writer,
        in ReadOnlySpan<char> key,
        in SourceCodeBinderCtx ctx
    ) where TSyntaxWriter : ISyntaxWriter;
}

internal sealed class InvocationExpressionSourceCodeBinder : ISourceCodeBinder {
    public static InvocationExpressionSourceCodeBinder Instance { get; } = new();

    public void WriteBindableSyntax<TSyntaxWriter>(
        in TSyntaxWriter writer,
        in ReadOnlySpan<char> key,
        in SourceCodeBinderCtx ctx
    ) where TSyntaxWriter : ISyntaxWriter {
        var actualKey = key[__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeStart.Length..];
        var invocation = AssertContext(ctx);
        if (actualKey is "Invoke") {
            WriteInvocation(in writer, invocation, in ctx);
            return;
        }
    }

    private void WriteInvocation<TSyntaxWriter>(
        in TSyntaxWriter writer,
        in InvocationExpressionSyntax invocation,
        in SourceCodeBinderCtx ctx
    ) where TSyntaxWriter : ISyntaxWriter {
        var sm = ctx.Compilation.GetSemanticModel(invocation.SyntaxTree);
        var method = (IMethodSymbol)sm.GetSymbolInfo(invocation).Symbol!;

        if (method.IsStatic | method.IsExtensionMethod) {
            writer.Write(method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }
        else {
            writer.Write("self");
        }

        writer.Write($".{method.Name}(");
    }

    private static InvocationExpressionSyntax AssertContext(
        in SourceCodeBinderCtx ctx
    ) {
        return ctx.BindingTarget as InvocationExpressionSyntax
               ?? throw new InvalidOperationException("Binding target is not an InvocationExpressionSyntax.");
    }
}

internal readonly ref struct SourceCodeBinderCtx {
    public required object? BindingTarget { get; init; }
    public required Compilation Compilation { get; init; }
}

internal readonly ref struct RoslynQtMethodFactory(
    QtCompilationScope scope
) {
    public QtMethod<CompileTimeUnknown> CreateProxyMethodForInvocation(
        InvocationExpressionSyntax invocationToProxy,
        __DynamicallyBoundSourceCode sourceCode,
        QtDeclarationPtr<QtMethodCore> declPtr
    ) {
        var bindingCtx = new SourceCodeBinderCtx {
            BindingTarget = invocationToProxy,
            Compilation = scope.Compilation
        };
        var binder = new DynamicSourceCodeBinder(sourceCode, InvocationExpressionSourceCodeBinder.Instance, ref bindingCtx);
        var syntaxWriter = new SyntaxWriter();
        binder.WriteSyntax(syntaxWriter);

        var method = new QtMethod<CompileTimeUnknown>(
            "QtProxy__" + ((MemberAccessExpressionSyntax)invocationToProxy.Expression).Name.Identifier.Text,
            AccessModifier.PublicStatic,
            QtType.ForRuntimeType(typeof(bool)),
            new QtTypeParameterList(),
            new QtParameterList([
                new QtParameter("first", QtType.ForRuntimeType<IEnumerable<int>>(), ParameterModifier.This),
                new QtParameter("second", QtType.ForRuntimeType<IEnumerable<int>>())
            ]),
            new StaticQtMethodRepresentation(syntaxWriter.ToString()),
            new QtAttributeList(),
            declPtr
        );

        return method;
    }
}