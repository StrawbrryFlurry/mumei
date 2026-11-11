namespace Mumei.CodeGen.Roslyn.Testing.Tests.SourceFileReferences;

public sealed class SourceReferenceGeneratorTests {
    [Fact]
    public void Test() {
        GeneratorTest.For<SourceReferenceGenerator>(compilation => {
            compilation.AddTypeReference(typeof(SourceFile));
            compilation.AddSource(
                $$"""
                  using Mumei.Tests.Nested;
                  using {{typeof(SourceFile):ns}};

                  namespace Mumei.Tests;

                  public sealed class Foo {
                      public void TypeReference() {
                          var r = SourceFile.For<_Reference>();
                      }
                   
                      private sealed class _Reference {
                          private ReferencedClass? _referenced;
                        
                          public int[] One() {
                              return "1,2,3,4".ToCharArray()
                                  .Where(x => x != ',')
                                  .Select(x => int.Parse(x.ToString()))
                                  .ToArray();
                          }
                          
                          public void _Init() {
                            _referenced = new ReferencedClass();
                          }
                          
                          public ReferencedClass2 Two() {
                              return new ReferencedClass2();
                          }
                      }
                  }

                  public sealed class ReferencedClass {
                    public {{typeof(ICompilationSourceComponent):q}} _externalAssembly = null!;
                  }
                  """
            ).AddSource(
                """
                namespace Mumei.Tests.Nested;

                public sealed class ReferencedClass2;
                """
            );
        }).RunWithAssert(result => {
            result.HasFileMatching("*.g.cs")
                .WithContent(
                    $$"""
                      file partial static class CompilerGeneratedSourceFileInterceptor {
                         [InterceptsLocation()]
                         public static {{typeof(DeclarationSourceComponent):g}} Intercept__For_|hash|<TReference>() {
                            return new {{typeof(DeclarationSourceComponent):g}}(
                                "_Reference",
                                {{Strings.RawStringLiteral}}
                                namespace Mumei.Tests;
                                
                                public sealed class _Reference {
                                    private ReferencedClass _referenced = null!;
                                  
                                    public int[] One() {
                                        return "1,2,3,4".ToCharArray()
                                            .Where(x => x != ',')
                                            .Select(x => int.Parse(x.ToString()))
                                            .ToArray();
                                    }
                                    
                                    public ReferencedClass2 Two() {
                                        return new ReferencedClass2();
                                    }
                                }
                                {{Strings.RawStringLiteral}},
                                new {{typeof(ICompilationSourceComponent):g}}[] {
                                    new {{typeof(DeclarationSourceComponent):g}}(
                                        "ReferencedClass",
                                        {{Strings.RawStringLiteral}}
                                        namespace Mumei.Tests;

                                        public sealed class ReferencedClass {
                                            public {{typeof(ICompilationSourceComponent):q}} _externalAssembly;
                                        }
                                        {{Strings.RawStringLiteral}},
                                        new {{typeof(ICompilationSourceComponent):g}}[] {
                                            new {{typeof(AssemblyCompilationSourceComponent):g}}(Assembly.Load("{{typeof(ICompilationSourceComponent):ns}}"))
                                        }                    
                                    ),
                                    new {{typeof(DeclarationSourceComponent):g}}(
                                      "ReferencedClass2",
                                      {{Strings.RawStringLiteral}}
                                      namespace Mumei.Tests.Nested;

                                      public sealed class ReferencedClass2;
                                      {{Strings.RawStringLiteral}},
                                      new {{typeof(ICompilationSourceComponent):g}}[] {
                                          new {{typeof(AssemblyCompilationSourceComponent):g}}(Assembly.Load("{{typeof(ICompilationSourceComponent):ns}}"))
                                      }                    
                                  )              
                                }
                            );
                         }
                      }
                      """
                );
        });
    }
}