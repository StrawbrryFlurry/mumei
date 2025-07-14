using Mumei.CodeGen.Playground.Qt;
using Mumei.CodeGen.Qt.Qt;
using Mumei.DependencyInjection.Injector.Behavior;

namespace Mumei.CodeGen.Playground;

using static Global;
using static AccessModifier;

file sealed class Example {
    public void Ex() {
        var modules = new QtField<IInjector>[10];

        var cls = new QtClass(Private | Static, "Foo");
        var parentField = cls.AddField<IInjector>(Private | Readonly, "_parent");

        var ijt = new InjectorTemplate(modules);
        cls.BindTemplateMethod(ijt, t => t.Get);

        var tProvider = TypeParam("TProvider");
        Old(cls, tProvider, parentField, modules);

        var actuallyT = new UnknownAssertionProxyTemplate();
    }

    private static void Old(QtClass cls, QtTypeParameter tProvider, QtField<IInjector> parentField, QtField<IInjector>[] modules) {
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

public sealed class UnknownAssertionProxyTemplate : QtDynamicInterceptorMethodTemplate {
    protected override Arg.TReturn Implementation() {
        throw new NotImplementedException();
    }
}

public sealed class ActuallyInvocationProxyTemplate : QtInterceptorMethodTemplate {
    public static T Actually<T>(T subject, string? because = null) {
        return subject;
    }
}

public interface IInjector {
    public IInjector Parent { get; }
    public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None);
    public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None);
    public bool TryGet(object token, IInjector scope, InjectFlags flags, out object? instance);
}