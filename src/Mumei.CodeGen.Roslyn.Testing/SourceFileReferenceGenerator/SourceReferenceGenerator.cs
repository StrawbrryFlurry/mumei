using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Roslyn.SourceProviders;

namespace Mumei.CodeGen.Roslyn.Testing;

[Generator]
public sealed class SourceReferenceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var invocations = context.InvocationsOf(SourceFile.For<Unbound>);

        context.RegisterSourceOutput(
            invocations.Collect(),
            (p, i) => {
                p.AddSource("SourceReferenceInterceptor.g.cs", "");
            }
        );
    }
}