using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public abstract class SyntheticClassDefinition<TSelf> where TSelf : new() {
    // Add an analyzer that ensures Synthetic Classes are never instantiated by user code!
    protected SyntheticClassDefinition() {
        // throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public virtual void DefineDynamicMembers() { }

    public ref SyntheticField<T> Field<T>(string name) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ref CompileTimeUnknown Field(string name) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TSelf DynamicNew(object[] args) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public Delegate Method(string name) {
        throw new NotSupportedException();
    }
    public TDef Method<TDef>(string name) where TDef : Delegate {
        throw new NotSupportedException();
    }

    internal void AddField(ISyntheticField field) { }
    internal void AddMethod(ISyntheticMethod method) { }
    internal void AddProperty(ISyntheticField property) { }
}

public abstract class SyntheticMethodDefinition {
    public T Invoke<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; } = null!;
    public MethodInfo Method { get; } = null!;
}

public interface IMethodBuilder {
    public T Arg<T>(string name);
    public CompileTimeUnknown Arg(string name, ITypeSymbol type);
}

public interface ISyntheticField { }

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

public abstract class SyntheticProperty : ISyntheticField {
    public SyntheticProperty(string parent, Type type) {
        throw new NotImplementedException();
    }
}

// Do AsyncLocal / ThreadLocal magic to check which class / type definition these members should be added
// Require that these methods are only called at valid callsites e.g. DefineDynamicMembers or inside other member definitions
public static class SyntheticMemberDeclarationFactory {
    private static AsyncLocal<SyntheticInjector.ISyntheticClassDefinition_MemberDeclarationBinder?> _activeBinder = new();

    internal static void SetActiveBinder(SyntheticInjector.ISyntheticClassDefinition_MemberDeclarationBinder? binder) {
        _activeBinder.Value = binder;
    }

    public static SyntheticField<T> DefineField<T>(string name) {
        if (_activeBinder.Value is not { } binder) {
            throw new InvalidOperationException();
        }

        return binder.DefineField<T>(name);
    }
    public static SyntheticField<CompileTimeUnknown> DefineField(Type type, string name) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
    public static SyntheticField<CompileTimeUnknown> DefineField(string name, ITypeSymbol type) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
    public static SyntheticField<CompileTimeUnknown> DefineField(string name, ISyntheticType type) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static SyntheticMethod<TSignature> DefineInterceptorMethod<TSignature>(InvocationExpressionSyntax invocationToProxy, TSignature impl) where TSignature : Delegate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static SyntheticMethod<TSignature> DefineMethod<TSignature>(string name, TSignature signature) where TSignature : Delegate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static void BindSyntheticImplementation(ISyntheticType member, ISyntheticType actualType) { }

    public static void Implement(Type baseType) { }
    public static void Implement(ISyntheticType baseType) { }
    public static void Implement(ITypeSymbol baseType) { }

    public static void Extend(Type baseType) { }
    public static void Extend(ISyntheticType baseType) { }
    public static void Extend(ITypeSymbol baseType) { }

    public static void DefineConstructor<TImplementaiton>(Delegate impl) { }
    public static void DefineConstructor(Action<IMethodBuilder> impl) { }
}

public static class SyntheticMemberAccessor { }

public sealed class SyntheticInjector : SyntheticClassDefinition<SyntheticInjector>, ISyntheticType {
    [Input]
    public ClassDeclarationSyntax InjectorClass { get; set; }

    [Input]
    public InvocationExpressionSyntax InjectInvocation { get; set; }

    [Input]
    public ITypeSymbol SomeType { get; set; }

    [Input]
    public SyntheticInjector ParentInjectorType { get; set; }

    [Output]
    public SyntheticInjector Parent { get; set; }

    public SyntheticField<Func<string>> FactoryField { get; set; }
    public SyntheticMethod<Action> InterceptInjectWhatever { get; set; }
    public SyntheticMethod<Action> SomethingDynamic { get; set; }

    public override void DefineDynamicMembers() {
        FactoryField = SyntheticMemberDeclarationFactory.DefineField<Func<string>>($"_factory_{InjectorClass.Identifier}}}");
        InterceptInjectWhatever = SyntheticMemberDeclarationFactory.DefineInterceptorMethod(InjectInvocation, InterceptWhatever);
        SomethingDynamic = SyntheticMemberDeclarationFactory.DefineMethod("SomethingDynamic", () => {
            Debug.WriteLine("SomethingDynamic" + InjectorClass.Identifier.Text);
        });

        SyntheticMemberDeclarationFactory.Implement(SomeType);

        SyntheticMemberDeclarationFactory.DefineConstructor(mb => {
            Field<string>("_name") = mb.Arg<string>("name");
            Field("_foo") = mb.Arg("foo", SomeType);
        });

        SyntheticMemberDeclarationFactory.BindSyntheticImplementation(Parent, ParentInjectorType);
    }

    public void InterceptWhatever() {
        Debug.Write("A");
    }

    [Output]
    public T Get<T>() {
        if (typeof(T) == typeof(Func<string>)) {
            // Any access to members outside their declaration doesn't matter to us since we can simply
            // emit the code e.g. the value of InjectorClass.Identifier.Text
            // as the field name here.
            return Field<T>(InjectorClass.Identifier.Text);
        }

        throw new NotSupportedException();
    }

    internal static class ComponentFactory__Impl {
        public static SyntheticClassBuilder<SyntheticInjector> Class<TDef>(Action<TDef> setup) where TDef : SyntheticClassDefinition<SyntheticInjector>, new() {
            var compilation = default(SyntheticCompilation);
            var definition = new TDef();
            setup(definition);
            var binder = new SyntheticInjector__SyntheticClass_DefinitionBinder(definition as SyntheticInjector);
            SyntheticMemberDeclarationFactory.SetActiveBinder(binder);
            definition.DefineDynamicMembers();
            binder.BindOutputMembers();
            SyntheticMemberDeclarationFactory.SetActiveBinder(null);
            return new SyntheticClassBuilder<SyntheticInjector>(compilation);
        }
    }

    // For each synthetic class declaration, generate a setup class that can map all the dynamic values
    internal sealed class SyntheticInjector__SyntheticClass_DefinitionBinder(SyntheticInjector syntheticDecl) : ISyntheticClassDefinition_MemberDeclarationBinder {
        private int __field_State = 0;
        private int __method_State = 0;

        public SyntheticField<T> DefineField<T>(string name) {
            // State machine esque implementation for all member types that maps the original declaration
            // in the DefineDynamicMembers method to the call site. We might need this in order to properly
            // map dynamic values e.g. inputs into the member info
            if (__field_State == 0) {
                return default!; // Track syntheticDecl.InjectorClass as required value to map to name
            }

            // We might not need this for constant members such as fields, but for things that require dynamic construction such as methods
            // where we do need to inject the method body from the syntax tree, we will need to render the method body in the binder here.
            throw new InvalidOperationException();
        }

        public void BindOutputMembers() {
            // syntheticDecl.AddProperty(new SyntheticProperty("Parent", typeof(SyntheticInjector)));
            // syntheticDecl.AddMethod(new SyntheticMethod("Public",  < T >, "Get", renderBody: (instance, tree) => {
            //     tree.Line("if (typeof(T) == typeof(Func<string>)) {");
            //     tree.Text("return ");
            //     tree.DynamicField(instance.InjectorClass.Identifier.Text);
            //     tree.Text(";");
            //     tree.Line("}");
            //     tree.Line("throw new NotSupportedException();");
            // }));
        }

        public SyntheticMethod<TSignature> DefineMethod<TSignature>(string name) {
            // if (__method_State == 0) {
            //     // Lets assume the interceptor method is just a method for now.
            //     var methodSignature = BindInterceptorMethodFromInvocation();
            //     var method = syntheticDecl.AddMethod(new SyntheticMethod(methodSignature, renderBody: (instance, tree) => {
            //         tree.Text("Debug.Write(\"A\");");
            //     }));
            //
            //     return method;
            // }
            //
            // if (__method_State == 1) {
            //     // SomethingDynamic method
            //     var method = syntheticDecl.AddMethod(new SyntheticMethod("Public",  <void >,
            //
            //     "SomethingDynamic", renderBody:
            //     (instance, tree) => {
            //         tree.Text("Debug.Write(\"SomethingDynamic\" + ");
            //         tree.Dynamic__Input(instance.InjectorClass.Identifier.Text);
            //         tree.Text(");");
            //     }));
            //
            //     return;
            // }

            throw new InvalidOperationException();
        }

        private object BindInterceptorMethodFromInvocation() {
            throw new NotImplementedException();
        }
    }

    internal interface ISyntheticClassDefinition_MemberDeclarationBinder {
        public SyntheticField<T> DefineField<T>(string name);

        public void BindOutputMembers();
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public sealed class OutputAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public sealed class InputAttribute : Attribute { }

public static class Usage {
    public static void Do() {
        // var def = ComponentFactory.Class<SyntheticInjector>(x => {
        //     x.InjectInvocation = null!;
        //     x.InjectorClass = null!;
        //     x.SomeType = null!;
        // });
//
        // var i = def.New(Array.Empty<object>());
        // i.Method("SomethingDynamic").DynamicInvoke(); // Emit invoke for that mehtod
        // i.Field<string>("_factory");
//
        // def.WithAccessModifier(AccessModifier.Public);
        // def.WithMethod( /* Method Builder here*/);
    }
}