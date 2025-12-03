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
                b.AllowInterceptorsIn(SyntaxTreeReferenceGenerator.InterceptorsNamespace);
                b.AddSource(
                    "foo",
                    """
                    using Mumei.Roslyn.Testing;

                    public sealed class Test {
                        public void TestMethod() {
                            _ = SyntaxTreeReference.Of(typeof(CompilationTestSource)); 
                        }
                    }

                    file sealed class CompilationTestSource {
                        public class Foo {
                            public Func<string> DoS { get; set; } = null!;
                        }
                        
                        public class Bar {
                            public string S { get; set; } = null!;
                        }
                    }
                    """
                ).WithAssemblyName("TestAssembly").AddTypeReference<CSharpCompilation>();
            }
        ).RunWithAssert(result => {
            result.HasFileMatching("*SyntaxTreeReferenceInterceptor__0.g.cs")
                .WithPartialContent(
                    $$""""""""""
                      namespace TestAssembly.CompilationSourceInterceptor {
                          internal static partial class SyntaxTreeReferenceInterceptor {
                              [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "*")]
                              public static {{typeof(ICompilationReference):g}} Intercept_Of__0(global::System.Type t) {
                                  return new {{typeof(RootCompilationReference):g}} {
                                      References = [
                                          new global::Mumei.Roslyn.Testing.SyntaxTreeCompilationReference {
                                              TypeName = "Foo",
                                              SourceCode = """""""""
                                              using global::System;
                                              
                                              public class Foo
                                              {
                                                  public Func<string> DoS { get; set; } = null !;
                                              }
                                              """"""""",
                                              References = [
                                                  new global::Mumei.Roslyn.Testing.AssemblyCompilationReference {
                                                      TypeName = "Func<string>",
                                                      AssemblyName = "System.Private.CoreLib, *",
                                                  },
                                              ]
                                          },
                                          new global::Mumei.Roslyn.Testing.SyntaxTreeCompilationReference {
                                              TypeName = "Bar",
                                              SourceCode = """""""""
                                              using global::System;
                                              
                                              public class Bar
                                              {
                                                  public string S { get; set; } = null !;
                                              }
                                              """"""""",
                                              References = [
                                                  new global::Mumei.Roslyn.Testing.AssemblyCompilationReference {
                                                      TypeName = "string",
                                                      AssemblyName = "System.Private.CoreLib, *",
                                                  },
                                              ]
                                          },
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
                        _ = SyntaxTreeReference.Of(typeof(CompilationTestSource));
                        _ = SyntaxTreeReference.Of<CompilationTestSource>();
                    }
                }

                file sealed class CompilationTestSource {
                    public class Foo {
                        public Func<string> DoS { get; set; } = null!;
                    }
                    
                    public class Bar {
                        public string S { get; set; } = null!;
                    }
                }
                """
            );
        }).RunWithAssert(result => {
            result.HasFileMatching("*SyntaxTreeReferenceInterceptor__2.g.cs")
                .WithPartialContent(
                    $$""""""""""
                      namespace TestAssembly.CompilationSourceInterceptor {
                          internal static partial class SyntaxTreeReferenceInterceptor {
                              [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "*")]
                              public static {{typeof(ICompilationReference):g}} Intercept_Of__2() {
                                  return new {{typeof(RootCompilationReference):g}} {
                                      References = [
                                          new global::Mumei.Roslyn.Testing.SyntaxTreeCompilationReference {
                                              TypeName = "Foo",
                                              SourceCode = """""""""
                                              using global::System;
                                              
                                              public class Foo
                                              {
                                                  public Func<string> DoS { get; set; } = null !;
                                              }
                                              """"""""",
                                              References = [
                                                  new global::Mumei.Roslyn.Testing.AssemblyCompilationReference {
                                                      TypeName = "Func<string>",
                                                      AssemblyName = "System.Private.CoreLib, *",
                                                  },
                                              ]
                                          },
                                          new global::Mumei.Roslyn.Testing.SyntaxTreeCompilationReference {
                                              TypeName = "Bar",
                                              SourceCode = """""""""
                                              using global::System;
                                              
                                              public class Bar
                                              {
                                                  public string S { get; set; } = null !;
                                              }
                                              """"""""",
                                              References = [
                                                  new global::Mumei.Roslyn.Testing.AssemblyCompilationReference {
                                                      TypeName = "string",
                                                      AssemblyName = "System.Private.CoreLib, *",
                                                  },
                                              ]
                                          },
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