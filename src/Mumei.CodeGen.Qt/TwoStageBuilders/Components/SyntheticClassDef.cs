using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public abstract class SyntheticClassDefinition<TSelf> : ISyntheticClass {
    public virtual void DefineDynamicMembers() { }

    public ref SyntheticField<T> Field<T>(string name) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ref CompileTimeUnknown Field(string name) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TSelf New() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public Delegate Method(string name) {
        throw new NotSupportedException();
    }
    public TDef Method<TDef>(string name) where TDef : Delegate {
        throw new NotSupportedException();
    }

    #region ISyntheticClass

    public ImmutableArray<ISyntheticMethod> Methods { get; }

    #endregion
}

public interface IMethodBuilder {
    public T Arg<T>(string name);
    public CompileTimeUnknown Arg(string name, ITypeSymbol type);
}

public abstract class SyntheticField<T> {
    public static implicit operator SyntheticField<T>(T value) {
        throw new NotSupportedException();
    }

    public static implicit operator T(SyntheticField<T> value) {
        throw new NotSupportedException();
    }
}

public abstract class SyntheticInput<T> {
    public static implicit operator SyntheticInput<T>(T value) {
        throw new NotSupportedException();
    }

    public static implicit operator T(SyntheticInput<T> value) {
        throw new NotSupportedException();
    }
}

public abstract class SyntheticMethod<TSignature> { }

// Do AsyncLocal / ThreadLocal magic to check which class / type definition these members should be added
// Require that these methods are only called at valid callsites e.g. DefineDynamicMembers or inside other member definitions
public static class MemberDeclarationFactory {
    public static SyntheticField<T> DefineField<T>(string name) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static SyntheticField<CompileTimeUnknown> DefineField(string name, ITypeSymbol type) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static SyntheticMethod<TSignature> DefineInterceptorMethod<TSignature>(InvocationExpressionSyntax invocationToProxy, TSignature impl) where TSignature : Delegate { }
    public static SyntheticMethod<TSignature> DefineMethod<TSignature>(string name, TSignature signature) where TSignature : Delegate { }
    public static void Implement(ITypeSymbol baseType) { }
    public static void Extend(ISyntheticType baseType) { }

    public static void DefineConstructor(Delegate impl) { }
    public static void DefineConstructor(Action<IMethodBuilder> impl) { }
}

public sealed class InjectorDef : SyntheticClassDefinition<InjectorDef> {
    [Input]
    public ClassDeclarationSyntax InjectorClass { get; set; }

    [Input]
    public InvocationExpressionSyntax InjectInvocation { get; set; }

    [Input]
    public ITypeSymbol SomeType { get; set; }

    public SyntheticField<Func<string>> FactoryField { get; set; }
    public SyntheticMethod<Action> InterceptInjectWhatever { get; set; }
    public SyntheticMethod<Action> SomethingDynamic { get; set; }

    public override void DefineDynamicMembers() {
        FactoryField = MemberDeclarationFactory.DefineField<Func<string>>($"_factory_{InjectorClass.Identifier}}}");
        InterceptInjectWhatever = MemberDeclarationFactory.DefineInterceptorMethod(InjectInvocation, InterceptWhatever);
        SomethingDynamic = MemberDeclarationFactory.DefineMethod("SomethingDynamic", () => { });

        MemberDeclarationFactory.Implement(SomeType);

        MemberDeclarationFactory.DefineConstructor((mb) => {
            Field<string>("_name") = mb.Arg<string>("name");
            Field("_foo") = mb.Arg("foo", SomeType);
        });
    }

    public void InterceptWhatever() { }

    [CompilationMember]
    public T Get<T>() {
        if (typeof(T) == typeof(Func<string>)) {
            // Any access to members outside their declaration doesn't matter to us since we can simply
            // emit the code e.g. the value of InjectorClass.Identifier.Text
            // as the field name here.
            return Field<T>(InjectorClass.Identifier.Text);
        }

        throw new NotSupportedException();
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public sealed class CompilationMemberAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public sealed class InputAttribute : Attribute { }

public static class Usage {
    public void Do() {
        var def = ComponentFactory.Class<InjectorDef>(x => {
            x.InjectInvocation = null!;
            x.InjectorClass = null!;
            x.SomeType = null!;
        });
        var i = def.New(Array.Empty<object>());
        i.Method("SomethingDynamic").DynamicInvoke(); // Emit invoke for that mehtod
        i.Field<string>("_factory");

        def.WithAccessModifier(AccessModifier.Public);
        def.WithMethod( /* Method Builder here*/);
    }
}