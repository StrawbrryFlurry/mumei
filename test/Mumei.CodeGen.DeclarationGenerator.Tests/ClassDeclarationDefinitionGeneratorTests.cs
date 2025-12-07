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
                                      φbuilder.ΦCompilerApi.Context.Type(typeof(string)),
                                      "OutputA",
                                      new {{typeof(SyntheticPropertyAccessorList):g}}(
                                          {{typeof(SyntheticPropertyAccessor):g}}.SimpleGetter,
                                          null
                                      )
                                  ).WithAccessibility(global::Mumei.CodeGen.AccessModifier.Public);
                                  
                                  φbuilder.DeclareProperty<global::System.Type>(
                                      φbuilder.ΦCompilerApi.Context.Type(typeof(global::System.Type)),
                                      "OutputB",
                                      new {{typeof(SyntheticPropertyAccessorList):g}}(
                                          {{typeof(SyntheticPropertyAccessor):g}}.SimpleGetter,
                                          null
                                      )
                                  ).WithAccessibility(global::Mumei.CodeGen.AccessModifier.Public);
                                  
                                  φbuilder.DeclareField<{{typeof(CompileTimeUnknown):g}}>(
                                      φbuilder.ΦCompilerApi.DynamicallyBoundType(nameof(TState)),
                                      "_state"
                                  ).WithAccessibility(global::Mumei.CodeGen.AccessModifier.Private);
                                  
                                  φbuilder.DeclareMethod<global::System.Delegate>("DoWorkAsync")
                                      .WithAccessibility(global::Mumei.CodeGen.AccessModifier.Public)
                                      .WithReturnType(φbuilder.ΦCompilerApi.Context.Type(typeof(global::System.Threading.Tasks.Task)))
                                      .WithParameters(
                                          φbuilder.ΦCompilerApi.Context.Parameter(φbuilder.ΦCompilerApi.DynamicallyBoundType(nameof(TState)),
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

    [OmitInReference]
    public sealed class TestClassDefinitionBinder<TState> {
        [Input]
        public string InputA { get; }

        public void InternalBindCompilerOutputMembers(ISyntheticClassBuilder<TestClassDefinition<TState>> classBuilder) {
            classBuilder.DeclareProperty<string>(
                classBuilder.ΦCompilerApi.Context.Type(typeof(string)),
                "OutputA",
                new SyntheticPropertyAccessorList {
                    Getter = SyntheticPropertyAccessor.SimpleGetter,
                    Setter = null
                }
            );

            classBuilder.DeclareProperty<Type>(
                classBuilder.ΦCompilerApi.Context.Type(typeof(Type)),
                "OutputB",
                new SyntheticPropertyAccessorList {
                    Getter = SyntheticPropertyAccessor.SimpleGetter,
                    Setter = null
                }
            );

            classBuilder.DeclareField<CompileTimeUnknown>(
                classBuilder.ΦCompilerApi.DynamicallyBoundType(nameof(TState)),
                "_state"
            );

            classBuilder.DeclareMethod<Func<TState, Task>>("DoWorkAsync")
                .WithAccessibility(AccessModifier.Public)
                .WithReturnType(typeof(Task))
                .WithParameters(
                    classBuilder.ΦCompilerApi.Context.Parameter(
                        classBuilder.ΦCompilerApi.DynamicallyBoundType(
                            nameof(TState)),
                        "state"
                    )
                ).WithTypeParameters()
                .WithBody(classBuilder.ΦCompilerApi.Context.Block(this, static (renderTree, ctx) => {
                    renderTree.Line("_state = state;");
                    renderTree.Text("Console.WriteLine(\"Doing work... {\"");
                    renderTree.Text(ctx.InputA);
                    renderTree.Text("});");
                    renderTree.Line("return global::System.Threading.Tasks.Task.CompletedTask;");
                }));
        }
    }
}