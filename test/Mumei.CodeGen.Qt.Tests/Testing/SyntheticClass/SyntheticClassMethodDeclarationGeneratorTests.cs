using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Tests.Setup;
using Mumei.CodeGen.Qt.TwoStageBuilders.Components;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.Testing;

public sealed class SyntheticClassMethodDeclarationGeneratorTests {
    [Fact]
    public void Test() {
        var result = new SourceGeneratorTest<SyntheticClassMethodDeclarationGenerator>(b =>
            b.AddReference(SourceCode.Of<CompilationTestSource>()).WithAssemblyName("TestAssembly")
        ).Run();

        var x = result.GeneratedTrees.First();
        SyntaxVerifier.Verify(x.GetText().ToString(), $"");
    }
}

file sealed class CompilationTestSource {
    // Include into the compilation
    private Delegate _ref = TypeExtensions.Bind__self;

    public void TestInvocation() {
        var c = default(SyntheticCompilation)!;
        var m = c.DeclareClass("Test").DeclareMethod<Action>("A");
        m.WithBody(() => {
            Console.WriteLine("Hey!");
        });

        m.WithBody(new { Input = "Hello" }, static input => () => {
            Console.WriteLine("Hey!");
        });
    }
}

public static class TypeExtensions {
    extension(ITypeSymbol typeSymbol) {
        public string Bind__self() {
            return "Haii! :)";
        }
    }
}