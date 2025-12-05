using System.Runtime.CompilerServices;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Roslyn.Testing;
using Mumei.Roslyn.Testing.Extensions;

namespace Mumei.CodeGen.DeclarationGenerator.IntegrationTests;

public sealed class InterceptMethodEdgeCaseTests {
    [Fact]
    public void InterceptMethod_WithGenericTypeParameter_ArgumentIsAnonymousType() {
        CodeGenerationTest.ForSource(SyntaxTreeReference.Of(typeof(TestScope)))
            .IncrementalGenerate((ctx, compilation) => {
                var cls = compilation.FindType<TestScope.TestClass>();
                var method = cls.GetMethod("InvokeDoSomething");
                var toIntercept = method.GetDeclaration().FindInvocationOf("DoSomething");

                var testClass = ctx.GlobalNamespace.DeclareClass("Test")
                    .WithAccessibility(AccessModifier.File + AccessModifier.Static);

                testClass
                    .DeclareInterceptorMethod("TestIntercept", toIntercept)
                    .WithBody(ctx.Block(renderTree => {
                        renderTree.Text("Console.WriteLine($\"Intercepted {input}!\");");
                    }));

                ctx.Emit("Test", testClass);
            }).AssertFile("Test", result => {
//                var input = {{typeof(Unsafe)}}.As<T, ΦAnonymousTypeAccessProxy_input>(ref φAnonymousType__input);
                result.WithPartialContent(
                    $$"""
                      file static class Test {
                          [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "*")]
                          internal static void TestIntercept<T>(
                              this global::Mumei.CodeGen.DeclarationGenerator.IntegrationTests.TestClass φthis,
                              T φAnonymousType__input
                          ) {
                              Console.WriteLine($"Intercepted {input}!");
                          }

                          private sealed class ΦAnonymousTypeAccessProxy_input {
                              public string Name { get; }
                              public int Value { get; }
                          }
                      }
                      """
                );
            });
    }
}

file static class TestScope {
    public sealed class TestClass {
        public void InvokeDoSomething() {
            var anonymous = new { Name = "Test", Value = 42 };
            DoSomething(anonymous);
        }

        public void DoSomething<T>(T input) { }

        private sealed class ΦAnonymousTypeAccessProxy_input {
            public string Name { get; }
            public int Value { get; }
        }
    }
}