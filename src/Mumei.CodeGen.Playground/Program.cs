using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Playground;
using static Global;
using static Mumei.CodeGen.Playground.AccessModifier;

_ = 1;

internal sealed class Generator {
    public void Generate(Compilation compilation, InvocationExpressionSyntax invocation) {
        using var _ = CodeGenProvider.InitializeScope(compilation);

        var ns = Declare.Namespace("Paramita.UsageTests.Paramita.Generated");

        var compilerServices = Declare.Namespace("System.Runtime.CompilerServices");
        var interceptsLocationAttribute = Declare.Class(
                ["file", "sealed"],
                "InterceptsLocationAttribute"
            )
            .WithCtor((int version, string data) => { })
            .WithAttribute<AttributeUsageAttribute>(Arg.Of(AttributeTargets.Method), Arg.OfNamed("AllowMultiple", true));
        compilerServices.WithMember(interceptsLocationAttribute);

        // Declare wrapper classes for things like the InvocationExpression
        // that declare actual use cases for the syntax builders
        // E.g. Create a type for proxy invocations that know how to write a proxy method
        // for the given invocation and exposes APIs that can be used by the builder implementation
        // to reflect those use cases e.g. 
        // (ProxyInvocationTarget t) => {
        //     try {
        //         var result = t.InvokeProxyMethod();
        //      } catch (Exception ex) {
        //     }
        // }
        // Would replace the t.InvokeProxyMethod() with the actual invocation of the method on the target.
        // E.g. if the invocation to be proxied is:
        //     SomeClass.SomeMethod(1, "test", CancellationToken.None);
        // The generated code would look like:
        //     var result = SomeClass.SomeMethod(1, "test", CancellationToken.None);

        var interceptorClass = Declare.Class(["file", "static"], "λActuallyInterceptor_A");
        // new Arg.ProxyThis(invocation),
        interceptorClass.WithMethod(
            ["public", "static"], // Public | Static,
            $"SetObjectSubjectContext_Actually_{interceptorClass.NextId}",
            (_) => { }
        );

        var c = Declare.Class(["public", "static"], "TestClass");
        c.WithMethod(["public"], typeof(void), "TestMethod", static () => { })
            .WithAttribute(
                interceptsLocationAttribute,
                Arg.Of(1),
                Arg.Of("9TUKmRMGupPnVy7x1YFdU5QDAABFbnVtZXJhYmxlRXh0ZW5zaW9uc1Rlc3RzLmNz")
            );
    }
}

internal static class Declare {
    public static NamespaceDecl Namespace(string name) {
        return default!;
    }

    public static ClassDecl Class(Placeholder[] modifiers, string name) {
        return default!;
    }

    public static Placeholder Literal(string value) {
        return default;
    }
}

internal sealed class ClassDecl {
    private int _idCounter = 0;
    public string NextId => $"{++_idCounter}";

    public MethodDecl WithMethod(Placeholder[] modifiers, Placeholder returnType, string name, Delegate impl) {
        return default!;
    }

    public MethodDecl WithMethod<TBindingCtx>(Placeholder[] modifiers, string name, Func<Arg.TThis, TBindingCtx, Arg.TThis> implRef,
        TBindingCtx binder = default!) {
        return default!;
    }

    public MethodDecl WithMethod(Placeholder[] modifiers, string name, Action<Arg.TThis> implRef) {
        return default!;
    }

    public MethodDecl WithInterceptorMethod(
        InvocationExpressionSyntax invocationToIntercept,
        string name,
        Action<InterceptorMethodCtx> implRef,
        object binder
    ) {
        return default!;
    }

    public class InterceptorMethodCtx { }

    public static implicit operator Placeholder[](ClassDecl decl) {
        return default;
    }

    public ClassDecl WithCtor(Delegate impl) {
        return this;
    }

    public ClassDecl WithAttribute<TAttribute>(params Placeholder[] args) {
        return this;
    }

    public ClassDecl WithAttribute(Placeholder classOrDecl, params Placeholder[] args) {
        return this;
    }
}

internal sealed class NamespaceDecl {
    public NamespaceDecl WithMember(object member) {
        return this;
    }
}

public readonly struct Placeholder {
    public object RoslynSyntaxImpl { get; }

    public static implicit operator Placeholder(string value) {
        return default;
    }

    public static implicit operator Placeholder(Type value) {
        return default;
    }
}

internal sealed class MethodDecl {
    public MethodDecl WithAttribute(ClassDecl classOrDecl, params Placeholder[] args) {
        return this;
    }
}


internal static class Global {
    public static TypeSyntaxOrType CompileTypeOf<T>(params ReadOnlySpan<TypeSyntaxOrType> typeArgs) {
        return default;
    }

    public static TypeSyntaxOrType CompileTypeEx = CompileTypeOf<Dictionary<string, Arg.T1>>(SyntaxFactory.ParseTypeName("Foo"));

    public static IQtParameter Param<T>(string? name = null) {
        return default;
    }

    public static IQtParameter Param(IQtType type, string? name = null) {
        return default;
    }

    public static QtTypeParameter TypeParam(string name) {
        return default;
    }

    public static IQtType Return<T>() {
        return default;
    }

    public static ITypeSymbol ReturnType(InvocationExpressionSyntax invocation) {
        var c = CodeGenProvider._compilation.Value ?? throw new InvalidOperationException();
        var sm = c.GetSemanticModel(invocation.SyntaxTree);
        return ((IMethodSymbol)sm.GetSymbolInfo(invocation).Symbol!).ReturnType;
    }

    public struct TypeSyntaxOrType {
        public static implicit operator TypeSyntaxOrType(TypeSyntax syntax) {
            return default;
        }

        public static implicit operator TypeSyntaxOrType(Type type) {
            return default;
        }
    }
}

public static class Arg {
    public sealed class TThis { };

    public sealed class TReturn { };

    public sealed class ProxyThis { };

    public sealed class T1;

    public sealed class T2;

    public sealed class T3;

    public sealed class T4;

    public static Placeholder Of<T>(T value) {
        return default;
    }

    public static Placeholder OfNamed<T>(string name, T value) {
        return default;
    }
}

internal static class CodeGenProvider {
    public static readonly AsyncLocal<Compilation> _compilation = new();

    public static IDisposable InitializeScope(Compilation compilation) {
        _compilation.Value = compilation;
        return default!;
    }
}

internal abstract class Thing { }

public class CompileTimeBound { }