using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Tests.Setup;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests;

public sealed class QtClassTestsDynamicInterceptorMethod {
    [Fact]
    public void BindDynamicTemplateInterceptMethod() {
        var compilation = new TestCompilationBuilder().AddReference(
            SourceCode.Of<Templates.BindDynamicTemplateInterceptMethod>()
        ).Build();

        QtCompilationScope.SetActiveScope(compilation);

        var cls = new QtClass(AccessModifier.FileStatic, "TestClass");
        var x = compilation.SyntaxTrees.First(x => x.FilePath == nameof(BindDynamicTemplateInterceptMethod));
        var invocation = x.GetRoot()
                .DescendantNodesAndSelf(x => x is not InvocationExpressionSyntax)
                .FirstOrDefault(x => x is InvocationExpressionSyntax)
            as InvocationExpressionSyntax;

        // < >SM:field__Get(this)
        // => _foo
        // < >SM:Invoke
        // => Enumerable.SequenceEqual(__first, __second)
        // state.field.Get(ctx.This);
        cls.BindDynamicTemplateInterceptMethod(
            invocation!,
            static ctx => {
                bool result;
                try {
                    result = (bool)ctx.Invoke<BooleanLike>();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    Console.WriteLine(new {
                        Arguments = ctx.InvocationArguments,
                        Method = ctx.Method
                    });
                    throw;
                }

                return result;
            }
        );

        SyntaxVerifier.VerifyRegex(
            cls,
            $$"""
              file static class TestClass {
                  public static bool QtProxy__SequenceEqual(this {{typeof(IEnumerable<int>):g}} λthis, {{typeof(IEnumerable<int>):g}} λsecond) {
                      bool result;
                      try {
                          result = (bool){{typeof(Enumerable):g}}.SequenceEqual<int>(λthis, λsecond);
                      } 
                      catch ({{typeof(Exception):g}} e) {
                          {{typeof(Console):g}}.WriteLine(e);
                          {{typeof(Console):g}}.WriteLine(new {
                              Arguments = [ λthis, λsecond ],
                              Method = [ANY]
                          });
                          throw;
                      }

                      return ctx.Return(result);
                  }
              }
              """
        );
    }
}

file static class Templates {
    public sealed class BindDynamicTemplateInterceptMethod {
        public void Invoke() {
            IEnumerable<int> x = [1, 2, 3];
            x.SequenceEqual([1, 2, 3]);
        }
    }
}