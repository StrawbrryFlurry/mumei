using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.CodeGen.Roslyn.RoslynCodeProviders;
using Mumei.Roslyn.Testing;

namespace Mumei.CodeGen.DeclarationGenerator.IntegrationTests;

public sealed class InterceptorMethodDeclarationDefinitionGeneratorTests {
    [Fact]
    public void Test1() {
        SourceGeneratorTest.MetaGeneratorTest(
            SyntaxTreeReference.Of(typeof(TestScope)),
            context => {
                var x = context.CreateQtProvider(
                    (syntax, _) => syntax is InvocationExpressionSyntax,
                    (sfc, _) => { return (InvocationExpressionSyntax)sfc.Node; }
                );

                var o = x.IncrementalGenerate((ctx, s) => {
                    var cls = ctx.GlobalNamespace.DeclareClass("TestClass");

                    cls.DeclareField<string>(ctx.Type(typeof(string)), "_state");
                    cls.DeclareMethod<Delegate>("SomeOtherMethod").WithBody(ctx.Block((re) => { })).WithReturnType(typeof(void));

                    var stringType = ctx.TypeFromCompilation<string>();
                    var targetType = ((IInvocationOperation) ctx.Compilation.GetSemanticModel(s.SyntaxTree).GetOperation(s)).TargetMethod.ReceiverType;
                    cls.DeclareInterceptorMethod<TestInterceptorMethodDefinition<CompileTimeUnknown>>(
                        "DoWorkAsync",
                        s,
                        methodDef => methodDef.DoWorkAsync,
                        (binder) => {
                            binder.TargetType = targetType;
                            binder.DoCompare = new InvocationExpressionFragment("a", "", []);
                        }
                    );

                    cls.DeclareMethod<Func<string>>("GetState")
                        .WithReturnType(stringType)
                        .WithAccessibility(AccessModifier.Public)
                        .WithBody(ctx.Block(renderTree => {
                            renderTree.Text("return _state;");
                        }));

                    ctx.EmitIncremental("A", cls);
                });

                context.RegisterCodeGenerationOutput(o);
            }
        ).RunWithAssert(result => {
            result.HasFileMatching("A").WithContent(
                $$""""""""""
                  internal class TestClass {
                      private string _state;

                       void SomeOtherMethod() {

                      }

                      public global::System.Threading.Tasks.Task DoWorkAsync(
                          string state
                      ) {
                              _state = state;
                              SomeOtherMethod();
                              global::System.Console.WriteLine($"Doing work... {"""""""""Hello!"""""""""}");
                              return global::System.Threading.Tasks.Task.CompletedTask;
                      }

                      public string GetState() {
                          return _state;
                      }
                  }

                  """"""""""
            );

            result.Compilation.PassesAssemblyAction(c => {
                var testClass = c.CreateUnsafeInstance<TestMethodDefinition<string>>("TestClass");
                c.Invoke(testClass, definition => definition.DoWorkAsync("TestState")).GetAwaiter().GetResult();
                var state = c.Invoke(testClass, definition => definition.GetState());
                Assert.Equal("TestState", state);
            });
        });
    }
}

public sealed partial class TestInterceptorMethodDefinition<TTarget> : SyntheticInterceptorMethodDefinition {
    [Input]
    public ITypeSymbol TargetType { get; set; }

    [Input]
    public InvocationExpressionFragment DoCompare { get; set; }

    public override void BindDynamicComponents() {
        this.Bind(typeof(TTarget), TargetType);
    }

    [Output]
    public bool DoWorkAsync(TTarget target, string arg1, int arg2) {
        try {
            var result = Invoke<bool>();
            if (!EmitExpression<bool>(DoCompare.WithArguments(nameof(result)))) {
                throw new InvalidOperationException("Work already done");
            }

            return result;
        } catch {
            Console.WriteLine($"Caught exception while invoking method {Method.Name} with {InvocationArguments.Length} arguments.");
        }

        return true;
    }
}

file static class TestScope {
    public sealed class ClassWithInvocation {
        public void SomeOtherMethod() {
            TryDoSomeWork("arg1", 42);
        }

        public bool TryDoSomeWork(string s, int i) {
            return true;
        }
    }
}