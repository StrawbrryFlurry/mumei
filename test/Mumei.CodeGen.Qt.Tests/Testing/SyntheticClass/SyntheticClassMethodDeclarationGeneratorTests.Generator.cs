using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Tests.Setup;
using Mumei.CodeGen.Qt.TwoStageBuilders.Components;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.Testing;

public sealed class SyntheticClassMethodDeclarationGeneratorTests_Generator {
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
    private Type[] _compilationIncludes = [typeof(DefaultRenderExpressionExtensions)];

    public void TestInvocation() {
        var c = default(SyntheticCompilation)!;
        var m = c.DeclareClass("Test").DeclareMethod<Action<ISyntheticClassBuilder<CompileTimeUnknown>>>("A");
        var field = typeof(CompilationTestSource).GetField(nameof(_compilationIncludes))!;
        m.WithBody(new { Field = field }, static state => defBuilder => {
            // HellO!
            defBuilder.DeclareField(state.Field.FieldType, state.Field.Name);
        });
    }
}