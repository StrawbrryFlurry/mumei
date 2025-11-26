using Mumei.CodeGen.Qt.Tests.Setup;

namespace Mumei.CodeGen.Qt.Tests.Testing;

public sealed class SyntaxTreeReferenceGeneratorTests {
    [Fact]
    public void Test() {
        var result = new SourceGeneratorTest<SyntaxTreeReferenceGenerator>(b =>
            b.AddSource(
                $$"""
                  public sealed class Test {
                      public void TestMethod() {
                          var result = SyntaxTreeReference.Of<CompilationTestSource>(); 
                      }
                  }

                  file sealed class CompilationTestSource {
                      public string s = null!;
                  }
                  """
            ).WithAssemblyName("TestAssembly")
        ).Run();

        result.HasFileMatching("*SyntaxTreeReferenceInterceptor.g.cs")
            .WithPartialContent(
                $$""""""""""
                  namespace Generated {
                  }
                  {{InterceptsLocationAttributeSource.Generated}}
                  """"""""""
            );
    }
}