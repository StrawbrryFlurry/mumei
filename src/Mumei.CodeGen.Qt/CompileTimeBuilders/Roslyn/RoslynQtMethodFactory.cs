using System.Diagnostics;
using System.Security;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.Roslyn;

// ReSharper disable once InconsistentNaming
public readonly ref struct __DynamicallyBoundSourceCode {
    private const string DynamicallyBoundSourceCodeStart = "<>SM:";
    private const string DynamicallyBoundSourceCodeEnd = "ɵ";

    public required string[] CodeTemplate { get; init; }

    public static bool IsDynamicSection(ReadOnlySpan<char> section) {
        return section.StartsWith(DynamicallyBoundSourceCodeStart);
    }

    public static ReadOnlySpan<char> GetDynamicSectionKey(ReadOnlySpan<char> section) {
        var start = DynamicallyBoundSourceCodeStart.Length;
        var key = section[start..];
        return key;
    }

    public static string MakeDynamicSection(string key) {
        return $"{DynamicallyBoundSourceCodeStart}{key}{DynamicallyBoundSourceCodeEnd}";
    }

    public static int FindDynamicSectionEnd(ReadOnlySpan<char> span) {
        return span.IndexOf(DynamicallyBoundSourceCodeEnd);
    }

    public static int FindDynamicSectionStart(ReadOnlySpan<char> span) {
        return span.IndexOf(DynamicallyBoundSourceCodeStart);
    }
}

public sealed class QtDynamicComponentBinderCollection : Dictionary<string, IQtTemplateBindable> { }

public abstract class QtDynamicSyntaxNodeBinderBase<TTargetNode>(
    TTargetNode node
) : IQtTemplateBindable where TTargetNode : SyntaxNode {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        var syntaxString = node.NormalizeWhitespace().ToString();
        writer.WriteFormattedLine($"(({typeof(TTargetNode):g}){typeof(SyntaxFactory):g}.{nameof(SyntaxFactory.ParseExpression)}(");
        writer.WriteLine(new string('"', 8));
        writer.WriteBlock(syntaxString);
        writer.WriteLine();
        writer.WriteLine(new string('"', 8));
        writer.WriteLine("))");
    }
}

file readonly ref struct DynamicSourceCodeBinder(
    in __DynamicallyBoundSourceCode code
) {
    private readonly string[] _template = code.CodeTemplate;

    public CodeBlockFragment Bind(ISourceCodeBindingContext ctx) {
        var writer = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
        var t = _template;
        foreach (var section in t) {
            if (__DynamicallyBoundSourceCode.IsDynamicSection(section)) {
                ctx.WriteBindableSyntax(ref writer, __DynamicallyBoundSourceCode.GetDynamicSectionKey(section));
                continue;
            }

            writer.Write(section);
        }

        var codeBlock = CodeBlockFragment.ForCode(writer.ToString());
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
        } else {
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

        TemplateBindingContext.Current.CodeGenFeatures.Require(CodeGenFeature.MethodReflection);

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
    QtParameterList parameterList,
    IMethodSymbol methodSymbol
) {
    public Unit BindInto<TSyntaxWriter>(ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        if (methodSymbol.IsStatic) {
            return Unit.Value;
        }

        if (!parameterList.TryGetThisParameter(out var thisParameter)) {
            throw new InvalidOperationException("Expected a 'this' parameter to be present in the method parameters, but it was not found.");
        }

        writer.Write(thisParameter.Name);
        return Unit.Value;
    }
}

internal readonly record struct TemplateBindingKey {
    public string Kind { get; }
    public string? Value { get; }

    private TemplateBindingKey(string kind, string? value) {
        Kind = kind;
        Value = value;
    }

    public static TemplateBindingKey Parse(string value) {
        return Parse(value.AsSpan());
    }

    public static TemplateBindingKey Parse(ReadOnlySpan<char> value) {
        var colonIndex = value.IndexOf(':');
        if (colonIndex == -1) {
            return new TemplateBindingKey(value.ToString(), null);
        }

        var kind = value[..colonIndex].ToString();
        var val = value[(colonIndex + 1)..].ToString();
        return new TemplateBindingKey(kind, val);
    }

    public static TemplateBindingKey For(string kind, string? value = null) {
        return new TemplateBindingKey(kind, value);
    }

    public static implicit operator string(TemplateBindingKey key) {
        return $"{key.Kind}:{key.Value}";
    }
}

internal static class ProxyMethodBindingKeys {
    public const string Invoke = "PxInvoke";
    public const string MethodInfo = "PxMethodInfo";
    public const string ArgumentList = "PxArguments";

    public const string This = "PxThis";
    public const string Member = "PxMemberAccess";
    public const string State = "PxStateAccess";
    public const string Parameter = "PxParameter";
}

// We could prolly code-gen a struct binding context struct for each bindable location
// that looks the same as the other binding contexts
internal sealed class DynamicQtComponentBinder(
    QtDynamicComponentBinderCollection binders
) {
    public const string BindDynamicComponent = "PxDynamicComponent";

    private readonly Dictionary<string, IQtTemplateBindable> _bindables = binders;

    public bool CanBind(ReadOnlySpan<char> key) {
        return _bindables.ContainsKey(key.ToString());
    }

    public Unit BindInto<TSyntaxWriter>(ReadOnlySpan<char> key, ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        var keyString = key.ToString();
        if (!_bindables.TryGetValue(keyString, out var bindable)) {
            throw new InvalidOperationException($"No bindable found for key '{keyString}'.");
        }

        writer.Write(bindable);
        return Unit.Value;
    }

    public static string CreateDynamicComponentBinderKey(string s) {
        return $"{BindDynamicComponent}:{s}";
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
        in QtDeclarationPtr<QtMethodRenderFragment> declPtr,
        QtDynamicComponentBinderCollection? dynamicQtComponentBinder = null
    ) {
        var methodSymbol = scope.GetMethodSymbol(invocationToProxy);

        var factory = new RoslynQtComponentFactory(scope);
        var parameters = factory.InterceptParametersFor(methodSymbol);
        var invocationBindingContext = new ProxyInvocationExpressionBindingContext(
            invocationToProxy,
            new ProxyInvocationCallSiteInvokeBinder(invocationToProxy, methodSymbol, parameters),
            new InvocationCallSiteArgumentBinder(parameters),
            new InvocationCallSiteMethodInfoBinder(methodSymbol),
            new ProxyInvocationThisBinder(parameters, methodSymbol),
            dynamicQtComponentBinder is null ? null : new DynamicQtComponentBinder(dynamicQtComponentBinder)
        );

        var binder = new DynamicSourceCodeBinder(sourceCode);
        var methodBody = binder.Bind(invocationBindingContext);

        var interceptLocationAttribute = factory.InterceptLocationFor(invocationToProxy);
        var method = new QtMethod<CompileTimeUnknown>(
            "QtProxy__" + methodSymbol.Name,
            AccessModifier.PublicStatic,
            QtType.ForRoslynType(methodSymbol.ReturnType),
            new QtTypeParameterList(), // Since this is a proxy method, all types need to be bound to the types at the call site
            parameters,
            methodBody,
            QtAttributeList.With(interceptLocationAttribute),
            declPtr
        );

        return method;
    }
}