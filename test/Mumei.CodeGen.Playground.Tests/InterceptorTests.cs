using Mumei.CodeGen.Playground.Generator;
using Mumei.CodeGen.Roslyn.Testing;

namespace Mumei.CodeGen.Playground.Tests;

public sealed class InterceptorTests {
    [Fact]
    public async Task Test() {
        // TODO: Maybe start with the tests / Implementation of the source Reference thingy haha
        GeneratorTest.For<AssertionGenerator>(x => x.AddSource($$"""
                                                                 using Mumei.CodeGen.Playground;
                                                                 public sealed class _TestSource {
                                                                     public void Assert_Boolean() {
                                                                         var x = true;
                                                                         x.Assert();
                                                                     }
                                                                 }
                                                                 """).AddTypeReference(typeof(AssertionExtensions)))
            .RunWithAssert(compilation => {
                compilation.HasFileMatching("*Assert.g.cs").WithContent($"file static class Assert {{}}");
            });
    }

    private sealed class _TestSource {
        public void Assert_Boolean() {
            var x = true;
            x.Assert();
        }
    }
}