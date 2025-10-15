﻿using Mumei.CodeGen.Qt.Tests.Setup;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.Testing;

public sealed class CodeFragmentGeneratorTests {
    [Fact]
    public void Test() {
        var result = new SourceGeneratorTest<CodeFragmentGenerator>(b =>
            b.AddReference(SourceCode.Of<CompilationTestSource>()).WithAssemblyName("TestAssembly")
        ).Run();

        result.HasFileMatching("*CodeFragments.g.cs")
            .WithPartialContent(
                $$""""""""""
                  namespace TestAssembly.Generated {
                      file sealed class *Intercept_CodeFragment* {
                          internal static {{typeof(CodeFragment)}} *Intercept_CodeFragment*() {
                              return {{typeof(CodeFragment)}}.{{nameof(CodeFragment.λCreate)}}("*"
                      int x = 1;
                      int y = 2;
                      int z = x + y;
                  "*"
                  );

                          }
                      }
                  }
                  """"""""""
            );
    }
}

file sealed class CompilationTestSource {
    public void TestInvocation() {
        var fragment = CodeFragment.Create(() => {
            var x = 1;
            var y = 2;
            var z = x + y;
        });
    }
}