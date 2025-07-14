using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Playground;

internal sealed class ProxyInvocationExpressionQt(
    InvocationExpressionSyntax invocation
) : IQt {
    public void InjectSyntax() {
        throw new NotImplementedException();
    }
}