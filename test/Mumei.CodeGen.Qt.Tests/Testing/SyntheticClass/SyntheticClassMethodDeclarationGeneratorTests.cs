using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Tests.Setup;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.Testing;

public sealed class SyntheticClassMethodDeclarationGeneratorTests {
    [Fact]
    public void Test() {
        var result = new SourceGeneratorTest<SyntheticClassMethodDeclarationGenerator>(b =>
            b.AddReference(SourceCode.Of<CompilationTestSource>()).WithAssemblyName("TestAssembly")
        ).Run();
    }
}

file sealed class CompilationTestSource {
    // Include into the compilation
    private Delegate _ref = TypeExtensions.Bind__self;

    public void TestInvocation() {
        var typeSymbol = default(ITypeSymbol)!;
        CallSomethingWithTypeSymbol(typeSymbol);
    }

    public void CallSomethingWithTypeSymbol(ITypeSymbol typeSymbol) { }
}

public static class TypeExtensions {
    extension(ITypeSymbol typeSymbol) {
        public string Bind__self() {
            return "Haii! :)";
        }
    }
}