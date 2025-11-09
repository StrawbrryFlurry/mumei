using System.Collections.Immutable;
using System.Linq.Expressions;
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

    public TClassDef New(Expression<Func<TClassDef>> constructorExpression) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TClassDef> WithName(string name) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TClassDef> WithModifiers(params ReadOnlySpan<SyntheticAccessModifier> modifiers) {
        throw new NotImplementedException();
    }

    public ISyntheticMethodBuilder DeclareInterceptorMethod<TMethodDefinition>(string name, InvocationExpressionSyntax invocationToIntercept, Action<TMethodDefinition> inputBinder, Func<TMethodDefinition, Delegate> methodSelector) where TMethodDefinition : SyntheticMethodDefinition, new() {
        throw new NotImplementedException();
    }

    public ISyntheticMethodBuilder DeclareInterceptorMethod<TMethodDefinition>(InvocationExpressionSyntax invocationToIntercept, string name, Action<TMethodDefinition> inputBinder, Func<TMethodDefinition, Delegate> methodSelector) where TMethodDefinition : SyntheticMethodDefinition, new() {
        throw new NotImplementedException();
    }

    public ISyntheticMethodBuilder DeclareInterceptorMethod(InvocationExpressionSyntax invocationToIntercept, string name) {
        throw new NotImplementedException();
    }
    public ISyntheticMethodBuilder DeclareMethod<TMethodDefinition>(string name, Action<TMethodDefinition> inputBinder, Func<TMethodDefinition, Delegate> methodSelector) where TMethodDefinition : SyntheticMethodDefinition, new() {
        throw new NotImplementedException();
    }
    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TMethodDefinition, TSignature>(string name, Action<TMethodDefinition> inputBinder, Func<TMethodDefinition, TSignature> methodSelector) where TMethodDefinition : SyntheticMethodDefinition, new() where TSignature : Delegate {
        throw new NotImplementedException();
    }
    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TSignature>(string name) where TSignature : Delegate {
        throw new NotImplementedException();
    }

    public string MakeUniqueName(string n) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TNew> WithDefinition<TNew>() where TNew : ISyntheticClass {
        throw new NotImplementedException();
    }

    public ISyntheticField<TField> DeclareField<TField>(string name) {
        throw new NotImplementedException();
    }

    public ISyntheticField<CompileTimeUnknown> DeclareField(Type type, string name) {
        throw new NotImplementedException();
    }

    public ISyntheticField<CompileTimeUnknown> DeclareField(ITypeSymbol type, string name) {
        throw new NotImplementedException();
    }

    public ISyntheticField<CompileTimeUnknown> DeclareField(ISyntheticType type, string name) {
        throw new NotImplementedException();
    }

    public void DeclareProperty(ITypeSymbol type, string name) {
        throw new NotImplementedException();
    }
    public ISyntheticClassBuilder<TClassDef> Bind<TTarget>(ITypeSymbol type) {
        throw new NotImplementedException();
    }
    public ISyntheticClassBuilder<TNested> DeclareNestedClass<TNested>(string name, Action<TNested> bindInputs) where TNested : ISyntheticClass {
        throw new NotImplementedException();
    }
}

public interface ISyntheticTypeInfo<T> {
    public T New(params object[] args);
    public T New(Expression<Func<T>> constructorExpression);
}

public interface ISyntheticClassBuilder<T> : ISyntheticMember, ISyntheticTypeInfo<T> {
    public ISyntheticClassBuilder<T> WithName(string name);

    public ISyntheticClassBuilder<T> WithModifiers(params ReadOnlySpan<SyntheticAccessModifier> modifiers);

    public ISyntheticMethodBuilder DeclareInterceptorMethod<TMethodDefinition>(
        string name,
        InvocationExpressionSyntax invocationToIntercept,
        Action<TMethodDefinition> inputBinder,
        Func<TMethodDefinition, Delegate> methodSelector
    ) where TMethodDefinition : SyntheticMethodDefinition, new();

    public ISyntheticMethodBuilder DeclareInterceptorMethod(
        InvocationExpressionSyntax invocationToIntercept,
        string name
    );

    public ISyntheticMethodBuilder DeclareMethod<TMethodDefinition>(
        string name,
        Action<TMethodDefinition> inputBinder,
        Func<TMethodDefinition, Delegate> methodSelector
    ) where TMethodDefinition : SyntheticMethodDefinition, new();

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TMethodDefinition, TSignature>(
        string name,
        Action<TMethodDefinition> inputBinder,
        Func<TMethodDefinition, TSignature> methodSelector
    ) where TMethodDefinition : SyntheticMethodDefinition, new() where TSignature : Delegate;

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TSignature>(
        string name
    ) where TSignature : Delegate;

    public string MakeUniqueName(string n);

    public ISyntheticField<TField> DeclareField<TField>(string name);
    public ISyntheticField<CompileTimeUnknown> DeclareField(Type type, string name);
    public ISyntheticField<CompileTimeUnknown> DeclareField(ITypeSymbol type, string name);
    public ISyntheticField<CompileTimeUnknown> DeclareField(ISyntheticType type, string name);
    void DeclareProperty(ITypeSymbol type, string name);

    public ISyntheticClassBuilder<T> Bind<TTarget>(ITypeSymbol type);

    public ISyntheticClassBuilder<TNested> DeclareNestedClass<TNested>(string name, Action<TNested> bindInputs) where TNested : ISyntheticClass;
}

public interface ISyntheticClass : ISyntheticType, ISyntheticMember {
    public ImmutableArray<ISyntheticMethod> Methods { get; }
}

public interface ISyntheticMethodBuilder<TSignature> : ISyntheticMethod<TSignature> where TSignature : Delegate {
    public ISyntheticMethodBuilder WithBody(TSignature bodyImpl);
    public ISyntheticMethodBuilder WithBody<TDeps>(TDeps deps, Func<TDeps, TSignature> bodyImpl);
    public ISyntheticMethodBuilder WithBody(ISyntheticCodeBlock body);
}

public interface ISyntheticMethodBuilder : ISyntheticMethod {
    public ISyntheticMethodBuilder WithName(string name);
    public ISyntheticMethodBuilder WithName(Func<ISyntheticClassBuilder<CompileTimeUnknown>, string> nameFactory);
    public ISyntheticMethodBuilder WithAccessibility(params ReadOnlySpan<SyntheticAccessModifier> modifiers);
    public ISyntheticMethodBuilder WithBody(ISyntheticCodeBlock body);
}

public interface ISyntheticMethod<TSignature> : ISyntheticMethod where TSignature : Delegate {
    public TSignature Bind(object target);
}

public interface ISyntheticMethod {
    public string Name { get; }

    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate;
}

internal sealed class RuntimeSyntheticMethod(MethodInfo method) : ISyntheticMethod {
    public string Name => method.Name;
    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate {
        throw new NotImplementedException();
    }

    public TSignature AsDelegate<TSignature>() where TSignature : Delegate {
        throw new NotImplementedException();
    }

    // The compiler will look for this method to construct the invocation of the synthetic method.
    public void Construct__Invoke( /* Compilation, InvocationCallSite (Statement?), InvocationNode */) { }
}

internal sealed class RoslynSyntheticMethod(IMethodSymbol method) : ISyntheticMethod {
    public string Name => method.Name;
    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate {
        throw new NotImplementedException();
    }
}

internal sealed class QtSyntheticMethod(string name) : ISyntheticMethod {
    public string Name { get; } = name;
    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate {
        throw new NotImplementedException();
    }
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