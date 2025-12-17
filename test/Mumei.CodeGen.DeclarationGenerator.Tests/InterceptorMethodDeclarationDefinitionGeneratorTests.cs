using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Roslyn.Testing;

namespace Mumei.CodeGen.DeclarationGenerator.Tests;

public sealed class InterceptorMethodDeclarationDefinitionGeneratorTests {
    [Fact]
    public void Test1() {
        SourceGeneratorTest.TestGenerator<DeclarationDefinitionGenerator>(
            c => c.AddReference(SyntaxTreeReference.Of(typeof(TestScope))).AddTypeReference<CSharpCompilation>().AddTypeReference<CSharpRendererSyntaxTree>()
        ).RunWithAssert(result => {
            result.HasFileMatching($"{nameof(TestScope.TestInterceptorMethodDefinition<>)}__0.g.cs")
                .WithPartialContent(
                    $$"""""""""""""
                      namespace Mumei.CodeGen.DeclarationGenerator.Tests {
                          public partial class TestInterceptorMethodDefinition<TTarget> {
                              public override global::Mumei.CodeGen.Components.ISyntheticInterceptorMethodBuilder<global::System.Delegate> InternalBindCompilerMethod(
                                  global::Mumei.CodeGen.Components.ISimpleSyntheticClassBuilder φbuilder,
                                  global::Microsoft.CodeAnalysis.Operations.IInvocationOperation φinvocationToBind,
                                  global::Microsoft.CodeAnalysis.CSharp.InterceptableLocation φlocationToIntercept,
                                  global::System.Delegate φtargetMethod
                              ) {
                                  if (φtargetMethod.Method == ((Delegate) DoWorkAsync).Method) {
                                      return ΦBindMethod__DoWorkAsync_T0_state(φbuilder);
                                  }
                                  throw new InvalidOperationException("Unsupported method");
                              }
                              
                              private global::Mumei.CodeGen.Components.ISyntheticInterceptorMethodBuilder<global::System.Delegate> ΦBindMethod__DoWorkAsync_T0_state(
                                  global::Mumei.CodeGen.Components.ISimpleSyntheticClassBuilder φbuilder
                              ) {
                                  return φbuilder.DeclareMethod<global::System.Delegate>("DoWorkAsync")
                                      .WithAccessibility(global::Mumei.CodeGen.AccessModifier.Public)
                                      .WithReturnType(this.CodeGenContext.Type(typeof(global::System.Threading.Tasks.Task)))
                                      .WithParameters(
                                          φbuilder.ΦCompilerApi.Context.Parameter(this.InternalResolveLateBoundType(nameof(TTarget)),
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

    [OmitInReference]
    public class TestMethodDefinition<TTarget> : SyntheticInterceptorMethodDefinition {
        [Input]
        public InvocationExpressionFragment DoCompare { get; set; }

        public bool DoWorkAsync(string arg1, int arg2) {
            return true;
        }

        public override ISyntheticInterceptorMethodBuilder<Delegate> InternalBindCompilerMethod(
            ISimpleSyntheticClassBuilder φbuilder,
            IInvocationOperation invocationToBind,
            InterceptableLocation locationToIntercept,
            Delegate φtargetMethod
        ) {
            if (φtargetMethod.Method == ((Delegate) DoWorkAsync).Method) {
                return ΦBindInterceptorMethod__DoWorkAsync_T0_state(
                    φbuilder,
                    locationToIntercept
                );
            }
            throw new InvalidOperationException("Unsupported method");
        }

        private ISyntheticInterceptorMethodBuilder<Delegate> ΦBindInterceptorMethod__DoWorkAsync_T0_state(
            ISimpleSyntheticClassBuilder φbuilder,
            InterceptableLocation location
        ) {
            var methodBuilder = φbuilder.ΦCompilerApi.DeclareInterceptorMethodBuilder("DoWorkAsync", φbuilder);

            return methodBuilder
                .WithAttributes(CodeGenContext.InterceptLocationAttribute(location))
                .WithAccessibility(AccessModifier.Public + AccessModifier.Static)
                .WithReturnType(CodeGenContext.Type(typeof(bool)))
                .WithParameters(
                    CodeGenContext.Parameter(
                        InternalResolveLateBoundType(nameof(TTarget)),
                        "target",
                        ParameterAttributes.This
                    ),
                    CodeGenContext.Parameter(
                        CodeGenContext.Type(typeof(string)),
                        "arg1"
                    ),
                    CodeGenContext.Parameter(
                        CodeGenContext.Type(typeof(int)),
                        "arg2"
                    )
                )
                .WithBody(
                    CodeGenContext.Block(
                        this,
                        static (φrenderTree, φctx) => {
                            φrenderTree.Text(
                                """"""""""""

                                """"""""""""
                            );

                            φrenderTree.Node(φctx.DoCompare.WithArguments("result"));

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