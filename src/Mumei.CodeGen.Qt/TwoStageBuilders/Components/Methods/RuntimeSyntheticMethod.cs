using System.Reflection;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class RuntimeSyntheticMethod(MethodInfo method) : ISyntheticMethod {
    public string Name => method.Name;

    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate {
        throw new NotImplementedException();
    }

    public TSignature AsDelegate<TSignature>() where TSignature : Delegate {
        throw new NotImplementedException();
    }
}