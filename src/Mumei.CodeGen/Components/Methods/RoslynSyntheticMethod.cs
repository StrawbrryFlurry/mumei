using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Components.Methods;

internal sealed class RoslynSyntheticMethod(IMethodSymbol method) : ISyntheticMethod {
    public string Name => method.Name;
    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate {
        throw new NotImplementedException();
    }
}