using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.Roslyn;

// ReSharper disable once InconsistentNaming
public readonly ref struct __DynamicallyBoundSourceCode {
    internal const string DynamicallyBoundSourceCodeStart = "<>SM:";
    internal const string DynamicallyBoundSourceCodeEnd = "ɵ";

    public required string[] CodeTemplate { get; init; }
}

file readonly ref struct DynamicSourceCodeBinder(
    in __DynamicallyBoundSourceCode code
) {
    private readonly string[] _template = code.CodeTemplate;

    public QtCodeBlock Bind(ISourceCodeBindingContext ctx) {
        var writer = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
        var t = _template;
        foreach (var section in t) {
            if (section.StartsWith(__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeStart)) {
                ctx.WriteBindableSyntax(ref writer, section[__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeStart.Length..]);
                continue;
            }

            writer.Write(section);
        }

        var codeBlock = new QtCodeBlock(writer.ToString());
        return codeBlock;
    }
}

internal interface ISourceCodeBindingContext {
    public void WriteBindableSyntax<TSyntaxWriter>(
        ref TSyntaxWriter writer,
        in ReadOnlySpan<char> key
    ) where TSyntaxWriter : ISyntaxWriter;
}

internal readonly struct ProxyInvocationCallSiteInvokeBinder(
    InvocationExpressionSyntax invocation,
    IMethodSymbol method,
    QtParameterList parameters
) {
    public Unit BindInto<TSyntaxWriter>(ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        var callIsOnThisObj = !method.IsExtensionMethod;
        var firstParameterIsThis = parameters[0].Attributes.HasFlag(ParameterAttributes.This);
        var parametersRequiredForInvocation = firstParameterIsThis && callIsOnThisObj ? parameters.Memory[1..] : parameters.Memory;

        if (method.IsStatic || method.IsExtensionMethod) {
            var declaringType = QtType.ForRoslynType(method.ContainingType);
            writer.Write(declaringType, "g");
        }
        else {
            Debug.Assert(callIsOnThisObj);
            var thisIdentifier = parameters[0].Name;
            writer.Write(thisIdentifier);
        }

        writer.Write(".");

        writer.Write(method.Name);
        if (method.TypeParameters.Length > 0) {
            var typeParameters = method.TypeArguments.AsMemory().RepresentAsSeparatedList(QtType.ForRoslynType);
            writer.WriteFormatted($"<{typeParameters}>");
        }

        writer.Write("(");
        var arguments = parametersRequiredForInvocation.RepresentAsSeparatedList(p => p.Name.Qt());
        writer.Write(arguments);
        writer.Write(")");

        return Unit.Value;
    }
}

internal readonly struct InvocationCallSiteMethodInfoBinder(
    IMethodSymbol method
) {
    public Unit BindInto<TSyntaxWriter>(ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteFormatted($"{typeof(MethodReflectionExtensions):g}.{nameof(MethodReflectionExtensions.GetMethod)}(");
        var declaringType = QtType.ForRoslynType(method.ContainingType);
        var typeParameters = method.TypeParameters.AsMemory().RepresentAsQtArray(QtType.ForRoslynType);
        var parameterTypes = method.Parameters.AsMemory().RepresentAsQtArray(p => QtType.ForRoslynType(p.Type));
        writer.WriteFormatted($"{declaringType:t}, {method.Name:q}, {typeParameters:t}, {parameterTypes:t}");

        return Unit.Value;
    }
}

internal readonly struct InvocationCallSiteArgumentBinder(
    QtParameterList methodParameters
) {
    public Unit BindInto<TSyntaxWriter>(ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        var argumentsArray = methodParameters.RepresentAsQtArray((QtParameter x) => x.Name.Qt());
        writer.Write(argumentsArray);

        return Unit.Value;
    }
}

internal readonly struct ProxyInvocationThisBinder(
    string? thisIdentifier,
    IMethodSymbol methodSymbol
) {
    public Unit BindInto<TSyntaxWriter>(ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        if (methodSymbol.IsStatic) {
            return Unit.Value;
        }

        writer.Write(thisIdentifier);
        return Unit.Value;
    }
}

// We could prolly code-gen a struct binding context struct for each bindable location
// that looks the same as the other binding contexts
internal sealed class DynamicQtComponentBinder {
    private readonly Dictionary<string, IQtTemplateBindable> _bindables = new();

    public bool CanBind(ReadOnlySpan<char> key) {
        return _bindables.ContainsKey(key.ToString());
    }

    public Unit BindInto<TSyntaxWriter>(ReadOnlySpan<char> key, ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        return Unit.Value;
    }
}

internal sealed class ProxyInvocationExpressionBindingContext(
    InvocationExpressionSyntax invocation,
    in ProxyInvocationCallSiteInvokeBinder invocationBinder,
    in InvocationCallSiteArgumentBinder argumentBinder,
    in InvocationCallSiteMethodInfoBinder methodInfoBinder,
    in ProxyInvocationThisBinder thisBinder,
    DynamicQtComponentBinder? dynamicQtComponentBinder = null
) : ISourceCodeBindingContext {
    public const string BindInvocation = "PxInvoke";
    public const string BindMethodInfo = "PxMethod";
    public const string BindArguments = "PxArguments";
    public const string BindThis = "PxThis";

    private readonly ProxyInvocationCallSiteInvokeBinder _invocationBinder = invocationBinder;
    private readonly InvocationCallSiteArgumentBinder _argumentBinder = argumentBinder;
    private readonly InvocationCallSiteMethodInfoBinder _methodInfoBinder = methodInfoBinder;
    private readonly ProxyInvocationThisBinder _thisBinder = thisBinder;

    public void WriteBindableSyntax<TSyntaxWriter>(
        ref TSyntaxWriter writer,
        in ReadOnlySpan<char> key
    ) where TSyntaxWriter : ISyntaxWriter {
        _ = key switch {
            BindInvocation => _invocationBinder.BindInto(ref writer),
            BindMethodInfo => _methodInfoBinder.BindInto(ref writer),
            BindArguments => _argumentBinder.BindInto(ref writer),
            BindThis => _thisBinder.BindInto(ref writer),
            _ when dynamicQtComponentBinder?.CanBind(key) ?? false => dynamicQtComponentBinder.BindInto(key, ref writer),
            _ => throw new NotSupportedException($"Could not bind '{key.ToString()}' while binding proxy invocation of '{invocation.ToString()}'.")
        };
    }
}

internal readonly ref struct RoslynQtMethodFactory(
    QtCompilationScope scope
) {
    public QtMethod<CompileTimeUnknown> CreateProxyMethodForInvocation(
        InvocationExpressionSyntax invocationToProxy,
        in __DynamicallyBoundSourceCode sourceCode,
        in QtDeclarationPtr<QtMethodCore> declPtr
    ) {
        var methodSymbol = scope.GetMethodSymbol(invocationToProxy);

        var factory = new RoslynQtComponentFactory(scope);
        var parameters = factory.ParametersOf(methodSymbol);
        var invocationBindingContext = new ProxyInvocationExpressionBindingContext(
            invocationToProxy,
            new ProxyInvocationCallSiteInvokeBinder(invocationToProxy, methodSymbol, parameters),
            new InvocationCallSiteArgumentBinder(parameters),
            new InvocationCallSiteMethodInfoBinder(methodSymbol),
            new ProxyInvocationThisBinder()
        );

        var binder = new DynamicSourceCodeBinder(sourceCode);
        var methodBody = binder.Bind(invocationBindingContext);

        var method = new QtMethod<CompileTimeUnknown>(
            "QtProxy__" + methodSymbol.Name,
            AccessModifier.FileStatic,
            QtType.ForRoslynType(methodSymbol.ReturnType),
            new QtTypeParameterList(), // Since this is a proxy method, all types need to be bound to the types at the call site
            parameters,
            methodBody,
            new QtAttributeList(),
            declPtr
        );

        return method;
    }
}