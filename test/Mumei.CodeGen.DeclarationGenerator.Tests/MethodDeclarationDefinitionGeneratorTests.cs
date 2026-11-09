using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Legacy.Components;
using Mumei.CodeGen.Legacy.Rendering;
using Mumei.CodeGen.Legacy.Roslyn.Components;
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
                      namespace Mumei.CodeGen.Legacy.DeclarationGenerator.Tests {
                          public partial class TestMethodDefinition<TState> {
                              public override global::Mumei.CodeGen.Legacy.Components.ISyntheticMethodBuilder<global::System.Delegate> InternalBindCompilerMethod(
                                  global::Mumei.CodeGen.Legacy.Components.ISimpleSyntheticClassBuilder φbuilder,
                                  global::System.Delegate φtargetMethod
                              ) {
                                  if (φtargetMethod.Method == ((Delegate) DoWorkAsync).Method) {
                                      return ΦBindMethod__DoWorkAsync_T0_state(φbuilder);
                                  }
                                  throw new InvalidOperationException("Unsupported method");
                              }
                              
                              private global::Mumei.CodeGen.Legacy.Components.ISyntheticMethodBuilder<global::System.Delegate> ΦBindMethod__DoWorkAsync_T0_state(
                                  global::Mumei.CodeGen.Legacy.Components.ISimpleSyntheticClassBuilder φbuilder
                              ) {
                                  return φbuilder.DeclareMethod<global::System.Delegate>("DoWorkAsync")
                                      .WithAccessibility(global::Mumei.CodeGen.Legacy.AccessModifier.Public)
                                      .WithReturnType(this.CodeGenContext.Type(typeof(global::System.Threading.Tasks.Task)))
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
                                                  global::Mumei.CodeGen.Legacy.Components.DefaultRenderExpressionExtensions.RenderExpression(φctx.InputA, φrenderTree);
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
}