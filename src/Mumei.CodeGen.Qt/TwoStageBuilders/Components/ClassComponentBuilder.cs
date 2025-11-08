using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class SyntheticClassBuilder<TClassDef>(SyntheticCompilation compilation) : ISyntheticClassBuilder<TClassDef> {
    public ImmutableArray<ISyntheticMethod> Methods { get; }

    public TClassDef New(object[] args) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TClassDef> WithName(string name) {
        throw new NotImplementedException();
    }
    public ISyntheticClassBuilder<TClassDef> WithAccessibility(params ReadOnlySpan<SyntheticAccessModifier> modifiers) {
        throw new NotImplementedException();
    }

    public ISyntheticMethodBuilder DeclareInterceptorMethod<TMethodDefinition>(InvocationExpressionSyntax invocationToIntercept, Action<TMethodDefinition> inputBinder, Func<TMethodDefinition, Delegate> methodSelector) where TMethodDefinition : SyntheticMethodDefinition, new() {
        throw new NotImplementedException();
    }
    public string MakeUniqueName(string n) {
        throw new NotImplementedException();
    }
}

public interface ISyntheticClassBuilder<T> {
    public ISyntheticClassBuilder<T> WithName(string name);

    public ISyntheticClassBuilder<T> WithAccessibility(params ReadOnlySpan<SyntheticAccessModifier> modifiers);

    public ISyntheticMethodBuilder DeclareInterceptorMethod<TMethodDefinition>(
        InvocationExpressionSyntax invocationToIntercept,
        Action<TMethodDefinition> inputBinder,
        Func<TMethodDefinition, Delegate> methodSelector
    ) where TMethodDefinition : SyntheticMethodDefinition, new();

    public string MakeUniqueName(string n);
}

public interface ISyntheticClass : ISyntheticType {
    public ImmutableArray<ISyntheticMethod> Methods { get; }
}

public interface ISyntheticMethodBuilder {
    public ISyntheticMethodBuilder WithName(string name);
    public ISyntheticMethodBuilder WithName(Func<ISyntheticClassBuilder<CompileTimeUnknown>, string> nameFactory);
    public ISyntheticMethodBuilder WithAccessibility(params ReadOnlySpan<SyntheticAccessModifier> modifiers);
}

public interface ISyntheticMethod {
    public string Name { get; }
}

internal sealed class RuntimeSyntheticMethod(MethodInfo method) : ISyntheticMethod {
    public string Name => method.Name;
}

internal sealed class RoslynSyntheticMethod(IMethodSymbol method) : ISyntheticMethod {
    public string Name => method.Name;
}

internal sealed class QtSyntheticMethod(string name) : ISyntheticMethod {
    public string Name { get; } = name;
}

public readonly struct SyntheticAccessModifier {
    public static readonly SyntheticAccessModifier Public = new("public");
    public static readonly SyntheticAccessModifier Private = new("private");
    public static readonly SyntheticAccessModifier Protected = new("protected");
    public static readonly SyntheticAccessModifier Internal = new("internal");
    public static readonly SyntheticAccessModifier File = new("file");
    public static readonly SyntheticAccessModifier Sealed = new("sealed");
    public static readonly SyntheticAccessModifier Readonly = new("readonly");
    public static readonly SyntheticAccessModifier Static = new("static");

    private readonly string _modifier;

    private SyntheticAccessModifier(string modifier) {
        _modifier = modifier;
    }

    public override string ToString() {
        return _modifier;
    }
}