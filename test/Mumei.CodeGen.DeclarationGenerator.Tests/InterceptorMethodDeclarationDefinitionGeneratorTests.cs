using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        SourceGeneratorTest.TestGenerator<DeclarationDefinitionGenerator>(c =>
            c.AddReference(SyntaxTreeReference.Of(typeof(TestScope))).AddTypeReference<CSharpCompilation>()
                .AddTypeReference<CSharpRendererSyntaxTree>()
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
                                      return ΦBindMethod__DoWorkAsync_T0_target_arg1_arg2(φbuilder, φlocationToIntercept);
                                  }
                                  throw new InvalidOperationException("Unsupported method");
                              }
                              
                              private global::Mumei.CodeGen.Components.ISyntheticInterceptorMethodBuilder<global::System.Delegate> ΦBindMethod__DoWorkAsync_T0_target_arg1_arg2(
                                  global::Mumei.CodeGen.Components.ISimpleSyntheticClassBuilder φbuilder,
                                  global::Microsoft.CodeAnalysis.CSharp.InterceptableLocation φlocationToIntercept
                              ) {
                                  return global::Mumei.CodeGen.Roslyn.Components.SyntheticClassBuilderExtensions.DeclareInterceptorMethodBuilder(φbuilder.ΦCompilerApi, "DoWorkAsync", φbuilder)
                                      .WithAttributes(global::Mumei.CodeGen.Roslyn.Components.CodeGenerationContextExtensions.InterceptLocationAttribute(this.CodeGenContext, φlocationToIntercept))
                                      .WithAccessibility(global::Mumei.CodeGen.AccessModifier.Public)
                                      .WithReturnType(this.CodeGenContext.Type(typeof(bool)))
                                      .WithBody(
                                          this.CodeGenContext.Block(
                                              this,
                                              static (φrenderTree, φctx) => {
                                                  φrenderTree.Text(""""""""""""
                                                      try
                                                      {
                                                          bool result = Invoke<bool>();
                                                          if (!global::Mumei.CodeGen.Components.SyntheticDeclarationDefinition.EmitExpression<bool>(DoCompare.WithArguments(nameof(result))))
                                                          {
                                                              throw new global::System.InvalidOperationException("Work already done");
                                                          }
                                                  
                                                          return result;
                                                      }
                                                      catch
                                                      {
                                                          global::System.Console.WriteLine($"Caught exception while invoking method {Method.Name} with {InvocationArguments.Length} arguments.");
                                                      }
                                                  
                                                      return true;
                                                  
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
            }
            catch {
                Console.WriteLine(
                    $"Caught exception while invoking method {Method.Name} with {InvocationArguments.Length} arguments.");
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
            InvocationExpressionSyntax invocationToIntercept,
            IInvocationOperation invocationOperation,
            Delegate φtargetMethod
        ) {
            if (φtargetMethod.Method == ((Delegate)DoWorkAsync).Method) {
                return ΦBindInterceptorMethod__DoWorkAsync_T0_state(
                    φbuilder,
                    invocationToIntercept,
                    invocationOperation
                );
            }

            throw new InvalidOperationException("Unsupported method");
        }

        private static readonly string[] DoWorkAsync_T0_state__Parameters = new[] { "target", "arg1", "arg2" };

        private ISyntheticInterceptorMethodBuilder<Delegate> ΦBindInterceptorMethod__DoWorkAsync_T0_state(
            ISimpleSyntheticClassBuilder φbuilder,
            InvocationExpressionSyntax invocationToIntercept,
            IInvocationOperation invocationOperation
        ) {
            var parameters = new[] { "target", "arg1", "arg2" };

            var method = φbuilder.DeclareInterceptorMethod("DoWorkAsync", invocationToIntercept);
            method.WithBody(
                CodeGenContext.Block(
                    (method, invocationOperation, ctx: this),
                    static (φrenderTree, inputs) => {
                        var φctx = inputs.ctx;
                        var method = inputs.method;
                        var invocationOperation = inputs.invocationOperation;

                        var parametersToMap = method.Parameters.AsSpan();
                        // Ignore the optional this parameter
                        if (
                            parametersToMap.Length > DoWorkAsync_T0_state__Parameters.Length
                            && parametersToMap[0].ParameterAttributes.HasFlag(ParameterAttributes.This)
                        ) {
                            parametersToMap = parametersToMap.Slice(1);
                        }

                        foreach (var parameter in parametersToMap) {
                            φrenderTree.Text("var ");
                            φrenderTree.Text(parameter.Name);
                            φrenderTree.NewLine();
                        }

                        φrenderTree.Text(
                            """"""""""""
                            var target = φthis;
                            var arg1 = s;
                            var arg2 = i;
                            """"""""""""
                        );

                        // Invoke<bool>()
                        // φrenderTree.Node(CompilerInterceptionReflector.InvokeMethodOperation(
                        //     method,
                        //     "Invoke",
                        //     method.ReturnType,
                        //     Array.Empty<ITypeSymbol>(),
                        //     Array.Empty<IOperation>(),
                        //     isGeneric: true
                        // ));

                        φrenderTree.Node(φctx.DoCompare.WithArguments("result"));

                        φrenderTree.Text(""""""""""""
                                         }");
                                             return global::System.Threading.Tasks.Task.CompletedTask;

                                         """""""""""");
                    }
                )
            );

            return method;
        }
    }
}