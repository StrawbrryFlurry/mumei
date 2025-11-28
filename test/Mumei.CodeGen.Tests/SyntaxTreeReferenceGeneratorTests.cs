using System.Linq.Expressions;

namespace Mumei.CodeGen.Tests;

public sealed class SyntaxTreeReferenceGeneratorTests {
    [Fact]
    public void Test() { }
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