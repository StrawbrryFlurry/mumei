using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Mumei.Roslyn.Testing.CompilationReferenceGenerator;
using static Mumei.Roslyn.Testing.SourceGeneratorTest;

namespace Mumei.Roslyn.Testing.Tests;

public sealed class SyntaxTreeReferenceGeneratorTests {
    [Fact]
    public void Test() {
        TestGenerator<SyntaxTreeReferenceGenerator>(
            b => {
                b.AddSource(
                    "foo",
                    """
                    using Mumei.Roslyn.Testing;

                    public sealed class Test {
                        public void TestMethod() {
                            var result = SyntaxTreeReference.Of<CompilationTestSource>(); 
                        }
                    }

                    file sealed class CompilationTestSource {
                        public string s = null!;
                    }
                    """
                ).WithAssemblyName("TestAssembly").AddTypeReference<CSharpCompilation>();
            }
        ).RunWithAssert(result => {
            result.HasFileMatching("*SyntaxTreeReferenceInterceptor_0__0.g.cs")
                .WithPartialContent(
                    $$""""""""""
                      namespace Generated {
                          internal static partial class SyntaxTreeReferenceInterceptor {
                              [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "*")]
                              public static global::Mumei.Roslyn.Testing.ICompilationReference Intercept_Of_0__0() {
                                  return new global::Mumei.Roslyn.Testing.SyntaxTreeCompilationReference {
                                      TypeName = "CompilationTestSource",
                                      SourceCode = "class root {}",
                                      References = [
                                      ]
                                  };
                              }
                          }
                      }

                      {{InterceptsLocationAttributeSource.Generated}}
                      """"""""""
                );
        }).UpdateCompilation(x => {
            x.UpdateFile(
                "foo",
                """
                using Mumei.Roslyn.Testing;

                public sealed class Test {
                    public void TestMethod() {
                        var result = SyntaxTreeReference.Of<CompilationTestSource>(); 
                        var result2 = SyntaxTreeReference.Of<CompilationTestSource>(); 
                    }
                }

                file sealed class CompilationTestSource {
                    public string s = null!;
                }
                """
            );
        }).RunWithAssert(result => {
            var t = result.GeneratedTrees;
            result.HasFileMatching("*SyntaxTreeReferenceInterceptor_0__1.g.cs")
                .WithPartialContent(
                    $$""""""""""
                      namespace Generated {
                          internal static partial class SyntaxTreeReferenceInterceptor {
                              [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "*")]
                              public static global::Mumei.Roslyn.Testing.ICompilationReference Intercept_Of_0__1() {
                                  return new global::Mumei.Roslyn.Testing.SyntaxTreeCompilationReference {
                                      TypeName = "CompilationTestSource",
                                      SourceCode = "class root {}",
                                      References = [
                                      ]
                                  };
                              }
                          }
                      }

                      {{InterceptsLocationAttributeSource.Generated}}
                      """"""""""
                );
        });

        var r = SyntaxTreeReference.Of(typeof(CompilationSource));
    }
}

file static class CompilationSource {
    public sealed class Foo {
        public TestReceivable TestInvocation() {
            var r = new TestReceivable();
            r.Invoke(s => s.Length > 0);
            return r;
        }
    }

    public sealed class TestReceivable {
        public string ParameterName { get; private set; } = null!;
        public string Body { get; private set; } = null!;

        public void Invoke(Func<string, bool> expression) { }

        public void ReceiveExpression(Expression<Func<string, bool>> expression) {
            ParameterName = expression.Parameters[0].Name;
            Body = expression.Body.ToString();
        }
    }
}