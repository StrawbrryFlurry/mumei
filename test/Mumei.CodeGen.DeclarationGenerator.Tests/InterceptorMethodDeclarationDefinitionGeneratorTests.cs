using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Roslyn.Testing;

namespace Mumei.CodeGen.DeclarationGenerator.Tests;

public sealed class InterceptorMethodDeclarationDefinitionGeneratorTests {
    [Fact]
    public void Test1() {
        SourceGeneratorTest.TestGenerator<DeclarationDefinitionGenerator>(
            SyntaxTreeReference.Of(typeof(TestScope))
        ).RunWithAssert(result => {
            result.HasFileMatching($"{nameof(TestScope.TestInterceptorMethodDefinition<>)}__0.g.cs")
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
    public sealed class ClassWithInvocation {
        public void SomeOtherMethod() {
            TryDoSomeWork("arg1", 42);
        }

        public bool TryDoSomeWork(string s, int i) {
            return true;
        }
    }

    public sealed partial class TestInterceptorMethodDefinition<TTarget> : SyntheticInterceptorMethodDefinition {
        [Input]
        public ITypeSymbol TargetType { get; }

        public override void BindDynamicComponents() {
            this.Bind(typeof(TTarget), TargetType);
        }

        [Output]
        public bool DoWorkAsync(TTarget target, string arg1, int arg2) {
            try {
                var result = Invoke<bool>();
                if (!result) {
                    throw new InvalidOperationException("Work already done");
                }

                return result;
            } catch {
                Console.WriteLine($"Caught exception while invoking method {Method.Name} with {InvocationArguments.Length} arguments.");
            }

            return true;
        }
    }

    [OmitInReference]
    public class TestMethodDefinition<TState> : SyntheticInterceptorMethodDefinition {
        public bool DoWorkAsync(string arg1, int arg2) {
            return true;
        }

        public override ISyntheticMethodBuilder<Delegate> InternalBindCompilerMethod(
            ISimpleSyntheticClassBuilder φbuilder,
            InvocationExpressionSyntax invocationToBind,
            Delegate φtargetMethod
        ) {
            if (φtargetMethod.Method == ((Delegate) DoWorkAsync).Method) {
                return ΦBindInterceptorMethod__DoWorkAsync_T0_state(
                    φbuilder,
                    invocationToBind
                );
            }
            throw new InvalidOperationException("Unsupported method");
        }

        private ISyntheticMethodBuilder<Delegate> ΦBindInterceptorMethod__DoWorkAsync_T0_state(
            ISimpleSyntheticClassBuilder φbuilder,
            IInvocationOperation invocationToBind,
            InterceptableLocation location
        ) {
            var method = φbuilder.ΦCompilerApi.DeclareInterceptorMethodBuilder("DoWorkAsync", φbuilder);
            return φbuilder.DeclareInterceptorMethod(
                    "DoWorkAsync",
                    invocationToBind
                )
                .WithAccessibility(AccessModifier.Public)
                .WithReturnType(CodeGenContext.Type(typeof(Task)))
                .WithParameters(
                    φbuilder.ΦCompilerApi.Context.Parameter(InternalResolveLateBoundType(nameof(TState)),
                        "target"
                    ),
                    φbuilder.ΦCompilerApi.Context.Parameter(InternalResolveLateBoundType(nameof(TState)),
                        "state"
                    ),
                    φbuilder.ΦCompilerApi.Context.Parameter(InternalResolveLateBoundType(nameof(TState)),
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
                            DefaultRenderExpressionExtensions.RenderExpression(φctx.InputA, φrenderTree);
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