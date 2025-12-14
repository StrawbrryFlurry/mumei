using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Roslyn.Testing;

namespace Mumei.CodeGen.DeclarationGenerator.Tests;

public sealed class MethodDeclarationDefinitionGeneratorTests {
    [Fact]
    public void Test1() {
        SourceGeneratorTest.TestGenerator<DeclarationDefinitionGenerator>(
            SyntaxTreeReference.Of(typeof(TestScope))
        ).RunWithAssert(result => {
            result.HasFileMatching($"{nameof(TestScope.TestMethodDefinition<>)}__0.g.cs")
                .WithPartialContent(
                    $$"""""""""""""
                      namespace Mumei.CodeGen.DeclarationGenerator.Tests {
                          public partial class TestMethodDefinition<TState> {
                              public override global::Mumei.CodeGen.Components.ISyntheticMethodBuilder<global::System.Delegate> InternalBindCompilerMethod(
                                  global::Mumei.CodeGen.Components.ISimpleSyntheticClassBuilder φbuilder,
                                  global::System.Delegate φtargetMethod
                              ) {
                                  if (φtargetMethod.Method == ((Delegate) DoWorkAsync).Method) {
                                      return ΦBindMethod__DoWorkAsync_T0_state(φbuilder);
                                  }
                                  throw new InvalidOperationException("Unsupported method");
                              }
                              
                              private global::Mumei.CodeGen.Components.ISyntheticMethodBuilder<global::System.Delegate> ΦBindMethod__DoWorkAsync_T0_state(
                                  global::Mumei.CodeGen.Components.ISimpleSyntheticClassBuilder φbuilder
                              ) {
                                  return φbuilder.DeclareMethod<global::System.Delegate>("DoWorkAsync")
                                      .WithAccessibility(global::Mumei.CodeGen.AccessModifier.Public)
                                      .WithReturnType(φbuilder.ΦCompilerApi.Context.Type(typeof(global::System.Threading.Tasks.Task)))
                                      .WithParameters(
                                          φbuilder.ΦCompilerApi.Context.Parameter(this.InternalResolveLateBoundType(nameof(TState)),
                                              "state"
                                          )
                                      )
                                      .WithBody(
                                          φbuilder.ΦCompilerApi.Context.Block(
                                              this,
                                              static (φrenderTree, φctx) => {
                                                  φrenderTree.Text(""""""""""""
                                                      _state = state;
                                                      SomeOtherMethod();
                                                      global::System.Console.WriteLine($"Doing work... {
                                                  """""""""""");
                                                  global::Mumei.CodeGen.Components.DefaultRenderExpressionExtensions.RenderExpression(φctx.InputA, φrenderTree);
                                                  φrenderTree.Text(""""""""""""
                                                  }");
                                                      return global::System.Threading.Tasks.Task.CompletedTask;
                                                  
                                                  """""""""""");
                                              }
                                          )
                                      );
                              }
                          }
                      }
                      """""""""""""
                );
        });
    }
}

file static class TestScope {
    public sealed partial class TestMethodDefinition<TState> : SyntheticMethodDefinition {
        [Input]
        public string InputA { get; }

        [Input]
        public ITypeSymbol InputB { get; }

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
    }

    [OmitInReference]
    public sealed class TestMethodDefinitionBinder<TState> {
        [Input]
        public string InputA { get; }

        public void GenerateMethod(
            ISimpleSyntheticClassBuilder classBuilder,
            Delegate targetMethod
        ) {
            if (targetMethod.Method == ((Delegate) DoWorkAsync).Method) {
                GenerateMethod__DoWorkAsync_T0_state(classBuilder);
                return;
            }

            throw new InvalidOperationException("Unsupported method");
        }

        public void GenerateMethod__DoWorkAsync_T0_state(ISimpleSyntheticClassBuilder classBuilder) {
            classBuilder.DeclareMethod<Func<TState, Task>>("DoWorkAsync")
                .WithAccessibility(AccessModifier.Public)
                .WithReturnType(typeof(Task))
                .WithParameters(
                    classBuilder.ΦCompilerApi.Context.Parameter(
                        classBuilder.ΦCompilerApi.DynamicallyBoundType(nameof(TState)),
                        "state"
                    )
                )
                .WithBody(classBuilder.ΦCompilerApi.Context.Block(this, static (renderTree, ctx) => {
                    renderTree.Line("_state = state;");
                    renderTree.Text("Console.WriteLine(\"Doing work... {\"");
                    renderTree.Text("ΦDoWorkAsync__Dep_SomeOtherMethod__1();");
                    renderTree.Text(ctx.InputA);
                    renderTree.Text("});");
                    renderTree.Line("return global::System.Threading.Tasks.Task.CompletedTask;");
                }));

            classBuilder.DeclareMethod<Func<TState, Task>>("ΦDoWorkAsync__Dep_SomeOtherMethod__1")
                .WithAccessibility(AccessModifier.Private)
                .WithReturnType(typeof(void))
                .WithBody(classBuilder.ΦCompilerApi.Context.Block(this, static (renderTree, ctx) => { }));
        }

        public Task DoWorkAsync(TState state) {
            return Task.CompletedTask;
        }
    }
}