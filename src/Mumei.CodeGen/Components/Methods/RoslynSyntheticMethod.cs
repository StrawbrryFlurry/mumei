using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Components;

internal sealed class RoslynSyntheticMethod(IMethodSymbol method) : ISyntheticMethod {
    public SyntheticIdentifier Name { get; } = method.Name;
    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate {
        throw new NotImplementedException();
    }
}