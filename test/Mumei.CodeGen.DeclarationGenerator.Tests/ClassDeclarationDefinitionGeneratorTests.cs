using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Roslyn.Testing;

namespace Mumei.CodeGen.DeclarationGenerator.Tests;

public sealed class ClassDeclarationDefinitionGeneratorTests {
    [Fact]
    public void Test1() {
        SourceGeneratorTest.TestGenerator<DeclarationDefinitionGenerator>(
            SyntaxTreeReference.Of(typeof(TestScope))
        ).RunWithAssert(result => {
            result.HasFileMatching($"{nameof(TestScope.TestClassDefinition<>)}__0.g.cs")
                .WithPartialContent(
                    $$"""""""""""""
                      namespace Mumei.CodeGen.DeclarationGenerator.Tests {
                          public partial class TestClassDefinition<TState> {
                              public override void InternalBindCompilerOutputMembers(
                                  {{typeof(ISyntheticClassBuilder<>):g}}<global::Mumei.CodeGen.DeclarationGenerator.Tests.TestClassDefinition<TState>> φbuilder
                              ) {
                                  φbuilder.DeclareProperty<string>(
                                      this.CodeGenContext.Type(typeof(string)),
                                      "OutputA",
                                      new {{typeof(SyntheticPropertyAccessorList):g}}(
                                          {{typeof(SyntheticPropertyAccessor):g}}.SimpleGetter,
                                          null
                                      )
                                  ).WithAccessibility(global::Mumei.CodeGen.AccessModifier.Public);
                                  
                                  φbuilder.DeclareProperty<global::System.Type>(
                                      this.CodeGenContext.Type(typeof(global::System.Type)),
                                      "OutputB",
                                      new {{typeof(SyntheticPropertyAccessorList):g}}(
                                          {{typeof(SyntheticPropertyAccessor):g}}.SimpleGetter,
                                          null
                                      )
                                  ).WithAccessibility(global::Mumei.CodeGen.AccessModifier.Public);
                                  
                                  φbuilder.DeclareField<{{typeof(CompileTimeUnknown):g}}>(
                                      this.InternalResolveLateBoundType(nameof(TState)),
                                      "_state"
                                  ).WithAccessibility(global::Mumei.CodeGen.AccessModifier.Private);
                                  
                                  φbuilder.DeclareMethod<global::System.Delegate>("DoWorkAsync")
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
    public sealed partial class TestClassDefinition<TState> : SyntheticClassDefinition<TestClassDefinition<TState>> {
        [Input]
        public string InputA { get; }

        [Input]
        public ITypeSymbol InputB { get; }

        [Output]
        public string OutputA { get; }

        [Output]
        public Type OutputB { get; }

        [Output]
        private TState _state;

        public override void Setup(ISyntheticClassBuilder<TestClassDefinition<TState>> classBuilder) {
            classBuilder.Bind(typeof(TState), InputB);
            // classBuilder.WithBaseClass(InputB);
        }

        [Output]
        public Task DoWorkAsync(TState state) {
            _state = state;
            Console.WriteLine($"Doing work... {InputA}");
            return Task.CompletedTask;
        }
    }
}