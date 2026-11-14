using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Roslyn.Testing.Tests;

public sealed class AdHoc {
    [Fact]
    public void Test() {
        Assert.True(EqualityComparer<Delegate>.Default.Equals(ExpressionReceiver(Foo.Do<string>), ExpressionReceiver(Foo.Do<string>)));
    }

    public Delegate ExpressionReceiver(Delegate expr) {
        return expr;
    }
}

public static class Foo {
    public static T Do<T>() {
        return default;
    }
}