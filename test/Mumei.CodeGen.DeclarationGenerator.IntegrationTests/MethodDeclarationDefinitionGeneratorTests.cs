using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.CodeGen.Roslyn.RoslynCodeProviders;
using Mumei.Roslyn.Testing;

namespace Mumei.CodeGen.DeclarationGenerator.IntegrationTests;

public sealed class MethodDeclarationDefinitionGeneratorTests {
    [Fact]
    public void Test1() {
        SourceGeneratorTest.MetaGeneratorTest(
            SyntaxTreeReference.Of(typeof(TestScope)),
            context => {
                var firstRun = true;
                var x = context.CreateQtProvider(
                    (a, b) => {
                        if (firstRun) {
                            firstRun = false;
                            return true;
                        }

                        return false;
                    },
                    (a, b) => { return ""; }
                );

                var o = x.IncrementalGenerate((ctx, s) => {
                    var cls = ctx.GlobalNamespace.DeclareClass("TestClass");

                    cls.DeclareField<string>(ctx.Type(typeof(string)), "_state");
                    cls.DeclareMethod<Delegate>("SomeOtherMethod").WithBody(ctx.Block((re) => { })).WithReturnType(typeof(void));

                    var stringType = ctx.TypeFromCompilation<string>();
                    cls.DeclareMethod<TestMethodDefinition<CompileTimeUnknown>>(
                        "DoWorkAsync",
                        methodDef => methodDef.DoWorkAsync,
                        (binder) => {
                            binder.InputA = "Hello!";
                            binder.InputB = stringType;
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

public sealed partial class TestMethodDefinition<TState> : SyntheticMethodDefinition {
    [Input]
    public string InputA { get; set; }

    [Input]
    public ITypeSymbol InputB { get; set; }

    private TState _state;

    public override void BindDynamicComponents() {
        this.Bind(typeof(TState), InputB);
    }

    [Output]
    public Task DoWorkAsync(TState state) {
        _state = state;
        SomeOtherMethod();
        Console.WriteLine($"Doing work... {InputA}");
        return Task.CompletedTask;
    }

    private void SomeOtherMethod() { }

    public TState GetState() {
        return _state;
    }
}

file static class TestScope { }