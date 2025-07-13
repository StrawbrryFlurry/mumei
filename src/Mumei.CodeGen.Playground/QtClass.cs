using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;
using static Mumei.CodeGen.Playground.AccessModifier;
using static Global;

namespace Mumei.CodeGen.Playground;

public interface IQtType : IQtTemplateBindable;

public interface IQtParameter : IQtType {
    public int Modifiers { get; }
}

public interface IQtInvokable<TReturn> : IQtTemplateBindable;

public interface IQtTemplateBindable;

public interface IQtCompileTimeValue<out T> {
    public T As<T>();
};

public interface IQtThis : IQtCompileTimeValue<Arg.TThis> { };

public sealed class QtClass : IQtType {
    public QtField<object> AddField(
        AccessModifier modifiers,
        IQtType type,
        string name
    ) {
        return null!;
    }

    public QtField<TField> AddField<TField>(
        AccessModifier modifiers,
        string name
    ) {
        return null!;
    }

    public QtField<object> Field(string name) {
        return null!;
    }

    public QtField<object> AddField(
        AccessModifier modifiers,
        IQtType type,
        string name,
        IQtCompileTimeValue<object> defaultValue
    ) {
        return null!;
    }

    public QtMethod<Unit> AddMethod<TBindingCtx>(
        AccessModifier modifiers,
        string name,
        QtTypeParameter[] typeParameters,
        IQtType[] parameters,
        Action<IQtThis, TBindingCtx, QtMethod.QtMethodBuilderCtx> implementation,
        TBindingCtx bindingCtx
    ) {
        return null!;
    }

    public QtMethod<object> AddMethod<TBindingCtx>(
        AccessModifier modifiers,
        IQtType returnType,
        string name,
        QtTypeParameter[] typeParameters,
        IQtType[] parameters, // Needs support for IQtParameter (out parammeters)
        Func<IQtThis, TBindingCtx, QtMethod.QtMethodBuilderCtx, object> implementation,
        TBindingCtx bindingCtx
    ) {
        return null!;
    }

    public QtMethod<TReturnType> AddMethod<TReturnType>(
        AccessModifier modifiers,
        string name,
        IQtType[] parameters,
        Func<IQtThis, QtMethod.QtMethodBuilderCtx, TReturnType> implementation
    ) {
        return null!;
    }

    public QtMethod<object> AddMethod(
        AccessModifier modifiers,
        string name,
        Delegate implementation
    ) {
        return null!;
    }

    public QtMethod<Unit> AddProxyMethod(
        AccessModifier modifiers,
        string name,
        Action<IQtThis, QtMethod.QtProxyMethodBuilderCtx> implementation
    ) {
        return null!;
    }

    public QtMethod<TReturnType> AddProxyMethod<TReturnType>(
        AccessModifier modifiers,
        string name,
        Func<IQtThis, QtMethod.QtProxyMethodBuilderCtx, TReturnType> implementation
    ) {
        return null!;
    }

    public QtMethod<object> AddProxyMethod(
        AccessModifier modifiers,
        IQtType returnType,
        string name,
        Func<IQtThis, QtMethod.QtProxyMethodBuilderCtx, object> implementation
    ) {
        return null!;
    }

    public QtMethod<object> BindTemplateMethod<TTemplate>(
        TTemplate template,
        Func<TTemplate, Delegate> methodSelector
    ) where TTemplate : QtClassTemplate<TTemplate> {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public sealed class QtMethod<TReturnType> : IQtInvokable<TReturnType> {
    public QtMethod<TReturnType> WithName(string name) {
        return this;
    }

    public QtMethod<TReturnType> WithTypeParameters(params QtTypeParameter[] typeParameters) {
        return this;
    }

    public QtMethod<TReturnType> WithParameters(params IQtParameter[] parameters) {
        return this;
    }

    public TReturnType Invoke(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TDynamicReturnType DynamicInvoke<TDynamicReturnType>(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public static class QtMethod {
    public sealed class QtProxyMethodBuilderCtx { }

    public sealed class QtMethodBuilderCtx {
        public MethodBuilderArgumentsProvider Arguments { get; }
        public MethodBuilderTypeArgumentsProvider TypeArguments { get; set; }

        public T Interpolate<T>(IQtSyntaxTemplateInterpolator interpolator) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }

        public void ForEach<T>(T[] source, Action<T, QtMethodBuilderCtx> action) where T : IQtTemplateBindable {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }

        public T Return<T>(T value) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }
    }

    public sealed class MethodBuilderArgumentsProvider {
        public T Inject<T>(int idx) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }

        public IQtCompileTimeValue<Arg.T1> Inject(IQtType type, int idx) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }
    }

    public sealed class MethodBuilderTypeArgumentsProvider {
        public Type Get(string name) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }
    }
}

public sealed class QtField<T> : IQtCompileTimeValue<T>, IQtTemplateBindable {
    public T Get(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TDynamic DynamicGet<TDynamic>(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void Set(IQtThis target, T value) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void DynamicSet<TDynamic>(IQtThis target, TDynamic value) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T1 As<T1>() {
        throw new NotImplementedException();
    }
}

public sealed class QtInterface : IQtType { }

public sealed class Unit;

file sealed class Example {
    public void Ex() {
        var modules = new QtField<IInjector>[10];

        var cls = new QtClass();
        var parentField = cls.AddField<IInjector>(Private | Readonly, "_parent");

        var ijt = new InjectorTemplate(modules);
        cls.BindTemplateMethod(ijt, t => t.Get);

        var tProvider = TypeParam("TProvider");
        cls.AddMethod(
            Public,
            Return<bool>(),
            "TryGet",
            [tProvider],
            [Param<IInjector?>("scope").WithDefault(null!), Param<InjectFlags>("flags").WithDefault(InjectFlags.None), Param(tProvider).Out()],
            static (@this, bCtx, ctx) => {
                var scope = ctx.Arguments.Inject<IInjector?>(0);
                scope ??= @this.As<IInjector>();
                var flags = ctx.Arguments.Inject<InjectFlags>(1);
                var token = bCtx.tProvider.TypeOf;
                var instance = ctx.Arguments.Inject(bCtx.tProvider, 2).As<object>();

                if ((flags & InjectFlags.SkipSelf) != 0) {
                    return bCtx.parentField.Get(@this).TryGet(token, scope, flags & ~InjectFlags.SkipSelf, out instance);
                }

                ctx.ForEach(bCtx.modules, (module, ctx) => {
                    if (module.Get(@this).TryGet(token, scope, flags, out instance)) {
                        ctx.Return(true);
                    }
                });

                instance = null!;
                return false;
            },
            new { parentField, tProvider, modules }
        );
    }
}

public class InjectorTemplate(
    QtField<IInjector>[] modules
) : QtClassTemplate<InjectorTemplate>, IInjector {
    public IInjector Parent { get; }

    public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
        throw new NotImplementedException();
    }

    public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
        throw new NotImplementedException();
    }

    public bool TryGet(object token, IInjector scope, InjectFlags flags, out object? instance) {
        if ((flags & InjectFlags.SkipSelf) != 0) {
            return Parent.TryGet(token, scope, flags & ~InjectFlags.SkipSelf, out instance);
        }

        foreach (var compModule in CompTimeIterate(modules)) {
            if (compModule.Get(this).TryGet(token, scope, flags, out instance)) {
                return true;
            }
        }

        instance = null!;
        return false;
    }
}

public abstract class QtClassTemplate<TSelf> : IQtThis where TSelf : QtClassTemplate<TSelf> {
    public IEnumerable<TE> CompTimeIterate<TE>(IEnumerable<TE> compileTimeIterableSelector) where TE : IQtTemplateBindable {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T As<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public abstract class QtInterceptorMethodTemplate {
    public T Invoke<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public Arg.TThis This { get; set; }
    public object[] InvocationArguments { get; set; }
    public MethodInfo Method { get; set; }
}

public interface IInjector {
    public IInjector Parent { get; }
    public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None);
    public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None);
    public bool TryGet(object token, IInjector scope, InjectFlags flags, out object? instance);
}

public static class QtBindableExtensions {
    public static IQtParameter Parameter(this Type type, string? name = null) {
        return null!;
    }

    public static IQtParameter Out<T>(this T bindable) where T : IQtTemplateBindable {
        return null!;
    }

    public static IQtParameter WithDefault<T>(this T bindable, object defaultValue) where T : IQtTemplateBindable {
        return null!;
    }
}

public sealed class QtTypeParameter : IQtType {
    public Type TypeOf { get; }

    public static implicit operator QtTypeParameter(string name) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}