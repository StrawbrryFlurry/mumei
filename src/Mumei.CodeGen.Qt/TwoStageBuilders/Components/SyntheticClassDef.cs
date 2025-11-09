using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public abstract class SyntheticClassDefinition<TSelf> : ISyntheticClass where TSelf : new() {
    // Add an analyzer that ensures Synthetic Classes are never instantiated by user code!
    protected SyntheticClassDefinition() {
        // throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public virtual void SetupDynamic(ISyntheticClassBuilder<TSelf> classBuilder) { }

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

    public TSignature Method<TSignature>(string name) where TSignature : Delegate {
        throw new NotSupportedException();
    }

    public IEnumerable<T> CompileTimeForEach<T>(IEnumerable<T> items) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public virtual void BindCompilerOutputMembers(ISyntheticClassBuilder<TSelf> classBuilder) { }

    [DoesNotReturn]
    protected void ThrowDynamicallyImplemented() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ImmutableArray<ISyntheticMethod> Methods { get; }
}

public abstract class SyntheticMethodDefinition { }

public abstract class SyntheticInterceptorMethodDefinition {
    public virtual void BindDynamicComponents(IMethodBuilder methodBuilder) { }

    public T Invoke<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; } = null!;
    public MethodInfo Method { get; } = null!;
}

public interface IMethodBuilder {
    public T Arg<T>(string name);
    public CompileTimeUnknown Arg(string name, ITypeSymbol type);
    public IMethodBuilder BindSyntheticType<TTarget>(ITypeSymbol type);
}

public interface ISyntheticField { }
public interface ISyntheticField<T> { }

public abstract class SyntheticField<T> {
    public static implicit operator SyntheticField<T>(T value) {
        throw new NotSupportedException();
    }

    public static implicit operator T(SyntheticField<T> value) {
        throw new NotSupportedException();
    }
}

// Do AsyncLocal / ThreadLocal magic to check which class / type definition these members should be added
// Require that these methods are only called at valid callsites e.g. DefineDynamicMembers or inside other member definitions
public static class SyntheticMemberDeclarationFactory {
    [ThreadStatic]
    private static SyntheticInjector.ISyntheticClassDefinition_MemberDeclarationBinder? _activeBinder;

    internal static void SetActiveBinder(SyntheticInjector.ISyntheticClassDefinition_MemberDeclarationBinder? binder) {
        _activeBinder = binder;
    }

    public static ISyntheticMethod<TSignature> DefineInterceptorMethod<TSignature>(InvocationExpressionSyntax invocationToProxy, TSignature impl) where TSignature : Delegate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static ISyntheticMethod<TSignature> DefineMethod<TSignature>(string name, TSignature signature) where TSignature : Delegate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static void BindSyntheticImplementation(ISyntheticType member, ISyntheticType actualType) { }
    public static void BindSyntheticImplementation(Type member, ISyntheticType actualType) { }
    public static void BindSyntheticImplementation(Type member, ITypeSymbol actualType) { }

    public static void Implement(Type baseType) { }
    public static void Implement(ISyntheticType baseType) { }
    public static void Implement(ITypeSymbol baseType) { }

    public static void Extend(Type baseType) { }
    public static void Extend(ISyntheticType baseType) { }
    public static void Extend(ITypeSymbol baseType) { }

    public static void DefineConstructor<TImplementaiton>(Delegate impl) { }
    public static void DefineConstructor(Action<IMethodBuilder> impl) { }

    private static SyntheticInjector.ISyntheticClassDefinition_MemberDeclarationBinder EnsureBinder() {
        if (_activeBinder is not { } binder) {
            throw new InvalidOperationException();
        }

        return binder;
    }
}

public static class SyntheticMemberAccessor { }

public sealed class SyntheticInjector : SyntheticClassDefinition<SyntheticInjector> {
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
    public ISyntheticMethod<Action> InterceptInjectWhatever { get; set; }
    public ISyntheticMethod<Action> SomethingDynamic { get; set; }

    public override void SetupDynamic(ISyntheticClassBuilder<SyntheticInjector> builder) {
        // FactoryField = SyntheticMemberDeclarationFactory.DefineField<Func<string>>($"_factory_{InjectorClass.Identifier}}}");
        // InterceptInjectWhatever = SyntheticMemberDeclarationFactory.DefineInterceptorMethod(InjectInvocation, InterceptWhatever);
        // SomethingDynamic = SyntheticMemberDeclarationFactory.DefineMethod("SomethingDynamic", () => {
        //     Debug.WriteLine("SomethingDynamic" + InjectorClass.Identifier.Text);
        // });
        //
        // SyntheticMemberDeclarationFactory.Implement(SomeType);
        //
        // SyntheticMemberDeclarationFactory.DefineConstructor(mb => {
        //     Field<string>("_name") = mb.Arg<string>("name");
        //     Field("_foo") = mb.Arg("foo", SomeType);
        // });
        //
        // SyntheticMemberDeclarationFactory.BindSyntheticImplementation(Parent, ParentInjectorType);
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
            // definition.DefineDynamicMembers(binder.ClassBuilder);
            binder.BindOutputMembers();
            SyntheticMemberDeclarationFactory.SetActiveBinder(null);
            return new SyntheticClassBuilder<SyntheticInjector>(compilation);
        }
    }

    // For each synthetic class declaration, generate a setup class that can map all the dynamic values
    internal sealed class SyntheticInjector__SyntheticClass_DefinitionBinder(SyntheticInjector syntheticDecl) : ISyntheticClassDefinition_MemberDeclarationBinder {
        public ISyntheticClassBuilder<CompileTimeUnknown> ClassBuilder { get; set; }

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

        public ISyntheticMethod<TSignature> DefineMethod<TSignature>(string name) where TSignature : Delegate {
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
        public ISyntheticClassBuilder<CompileTimeUnknown> ClassBuilder { get; }

        public void BindOutputMembers();
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.GenericParameter | AttributeTargets.Constructor)]
public sealed class OutputAttribute : Attribute {
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class InputAttribute : Attribute;

/// <summary>
/// Declares a type parameter as bindable in a synthetic type definition
/// meaning it is not part of the actual type declaration but instead
/// will resolve to a concrete type when the type is constructed.
/// </summary>
[AttributeUsage(AttributeTargets.GenericParameter)]
public sealed class BindableAttribute : Attribute;

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