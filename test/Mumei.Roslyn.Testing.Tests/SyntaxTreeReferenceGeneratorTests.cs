using Mumei.Roslyn.Testing.SourceFileGenerator;
using static Mumei.Roslyn.Testing.SourceGeneratorTest;

namespace Mumei.Roslyn.Testing.Tests;

public sealed class SyntaxTreeReferenceGeneratorTests {
    [Fact]
    public void Test() {
        TestGenerator<SyntaxTreeReferenceGenerator>(
            b => {
                b.AddSource(
                    """
                    public sealed class Test {
                        public void TestMethod() {
                            var result = SyntaxTreeReference.Of<CompilationTestSource>(); 
                        }
                    }

                    file sealed class CompilationTestSource {
                        public string s = null!;
                    }
                    """
                ).WithAssemblyName("TestAssembly");
            }
        ).RunWithAssert(result => {
            result.HasFileMatching("*SyntaxTreeReferenceInterceptor.g.cs")
                .WithPartialContent(
                    $$""""""""""
                      namespace Generated {
                      }
                      {{InterceptsLocationAttributeSource.Generated}}
                      """"""""""
                );
        }).UpdateCompilation(c => { });
    }
}