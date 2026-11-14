using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Roslyn.SourceProviders;

namespace Mumei.CodeGen.Roslyn.Testing.Tests.SourceProviders;

public sealed class InvocationSourceProviderExtensionsTests {
    [Fact]
    public void Test() {
        GeneratorTest.ForAdHoc(
            comp => {
                comp
                    .AddTypeReference(typeof(SourceFile))
                    .AddSource("class Test;")
                    .AddFragment(
                        $$"""
                          _ = {{typeof(SourceFile)}}.For<Test>();
                          """
                    );
            },
            ctx => {
                var x = ctx.InvocationsOf(SourceFile.For<Unbound>).WithTrackingName("Invocations");
                var y = 1;
                ctx.RegisterSourceOutput(x, (ctx, m) => {
                    ctx.AddSource($"{y++}", "");
                });
            }).RunWithAssert(result => {
            var x = result.RunResult.Results[0].TrackedSteps[""][0];
        });
    }
}