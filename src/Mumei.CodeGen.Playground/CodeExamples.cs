using Mumei.CodeGen.Qt;
using Mumei.CodeGen.Qt.Qt;
using Mumei.DependencyInjection.Injector.Behavior;

namespace Mumei.CodeGen.Playground;

using static Global;
using static AccessModifier;

file sealed class Example {
    public void Ex() {
        var modules = new QtField<IInjector>[10];

        var cls = new QtClass(PrivateStatic, "Foo");
        var parentField = cls.AddField<IInjector>(Private | Readonly, "_parent");

        var ijt = new InjectorTemplate(modules);
        cls.BindTemplateMethod(ijt, t => t.Get);

        var tProvider = TypeParam("TProvider");
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