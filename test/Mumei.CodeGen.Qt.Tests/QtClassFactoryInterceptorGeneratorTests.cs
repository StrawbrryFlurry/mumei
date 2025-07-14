using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Tests.Setup;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests;

public sealed class QtClassFactoryInterceptorGeneratorTests {
    [Fact]
    public void QtClasss() {
        var cls = new QtClass(AccessModifier.PublicSealed, "TestClass");
        var f = cls.AddField<string>(AccessModifier.PrivateReadonly, "_testField");

        SyntaxVerifier.Verify(
            cls,
            $$"""
              public sealed class TestClass {
                  private readonly {{typeof(string):g}} _testField;
              }
              """
        );
    }

    [Fact]
    public void Test1() {
        // SourceCode.Of<Test1Templates.Test1>();
        var r = new SourceGeneratorTest<QtClassFactoryInterceptorGenerator>().Run();
    }

    public static class Test1Templates {
        public class Test1 {
            public static void TestMethod() {
                var cls = new QtClass(AccessModifier.Public, "Test1");
                var f = cls.AddField<string>(AccessModifier.Private | AccessModifier.Readonly, "_testField");
            }
        }
    }
}