using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Roslyn;

#pragma warning disable
namespace Mumei.CodeGen.Playground;

file static class __QtInterceptorImpl_Renderer {
    private sealed class ContextStateAccessor_Example {
        public LambdaExpressionSyntax Lambda { get; }
    }

    // internal static class Renderer {
    //     public delegate void Render(IRenderer renderer, object ctx);
    // }

    internal sealed class ProxyMethodRenderContextWithState<TState>(InvocationExpressionSyntax proxyInvocation, TState state) {
        public TState State { get; } = state;

        public MethodInfoRenderNode MethodInfo() {
            return default;
        }

        public ThisRenderNode This() {
            return default;
        }

        public readonly record struct MethodInfoRenderNode : IRenderNode {
            public void Render(IRenderer renderer) { }
        }

        public readonly record struct ThisRenderNode : IRenderNode {
            public void Render(IRenderer renderer) { }
        }
    }

    private readonly struct Intercept__MethodBodyTemplate_Example(ProxyMethodRenderContextWithState<ContextStateAccessor_Example> ctx) : ICodeBlock {
        // Possibly explore something like this so we could treat the
        // class as a singleton / pass an Action<IRenderer, object>
        // to consumers
        public void Render(IRenderer r, object ctxArg) {
            if (ctxArg is not ProxyMethodRenderContextWithState<ContextStateAccessor_Example> ctx_) {
                throw new InvalidOperationException("Invalid context type passed to renderer");
            }

            _ = ctx_;
        }

        public void Render(IRenderer r) {
            r.Text(
                """"""""""""
                global::Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax px = (global::Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax) global::Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression(global::Microsoft.CodeAnalysis.SyntaxNodeExtensions.NormalizeWhitespace(
                """"""""""""
            );
            r.SyntaxNode(ctx.State.Lambda);
            r.Text(
                """"""""""""
                ).ToFullString());
                global::Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax[] statements = px.Body is global::Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block
                    ? [..block.Statements]
                    : [global::Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement((global::Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax) px.Body)];

                global::Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax[] parameters = px is global::Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax spx ? [spx.Parameter]
                    : px is global::Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax pplx ? [..pplx.ParameterList.Parameters]
                    : [];

                // We can replace the hardcoded Func type once we re-write this to use a method template which can be generic
                global::Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.RoslynAsExpressionReplacement.RoslynExpression<global::System.Func<string, bool>> x = new global::Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.RoslynAsExpressionReplacement.RoslynExpression<global::System.Func<string, bool>> {
                    Parameters = parameters,
                    Statements = statements
                };


                """"""""""""
            );
            r.Node(ctx.This());
            r.Text(
                """"""""""""
                .ReceiveExpression(x);

                """"""""""""
            );
        }
    };

    internal interface ICodeBlock : IRenderNode;

    internal readonly record struct RenderNode<T>(Action<IRenderer, T> RenderFn, T RenderState) : IRenderNode {
        public void Render(IRenderer renderer) {
            RenderFn(renderer, RenderState);
        }
    }

    public static QtMethod<CompileTimeUnknown> Intercept<TTemplateReferences>(
        in this QtClass @this,
        InvocationExpressionSyntax invocationToProxy,
        TTemplateReferences references,
        DeclareQtInterceptorVoidMethodWithRefs<TTemplateReferences> declaration
    ) {
        var state = Unsafe.As<TTemplateReferences, ContextStateAccessor_Example>(ref references);
        var bindingContext = new ProxyMethodRenderContextWithState<ContextStateAccessor_Example>(invocationToProxy, state);
        var template = new Intercept__MethodBodyTemplate_Example(bindingContext);

        return @this.__BindDynamicTemplateInterceptMethod(
            invocationToProxy,
            default
            // (__DynamicallyBoundSourceCode) (dynamic) new Renderer.Render(template.Render)
        );
    }
}

file static class __QtInterceptorImpl {
    private sealed class QtArgStateAccessor {
        public LambdaExpressionSyntax Lambda { get; }
    }

    private sealed class QtArgStateBinder_Lambda(LambdaExpressionSyntax node) : QtDynamicSyntaxNodeBinderBase<LambdaExpressionSyntax>(node) { }

    private static readonly string[] CachedSourceCodeTemplate_Intercept_99b72c1c902541378198fd3c39e2f9f1 = [
        """"""""""""
        global::Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax px = (global::Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax) global::Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression(global::Microsoft.CodeAnalysis.SyntaxNodeExtensions.NormalizeWhitespace(
        """""""""""",
        """"""""""""
        <>SM:PxDynamicComponent:QtArgState:Lambda
        """""""""""",
        """"""""""""
        ).ToFullString());
        global::Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax[] statements = px.Body is global::Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax block
            ? [..block.Statements]
            : [global::Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement((global::Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax) px.Body)];

        global::Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax[] parameters = px is global::Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax spx ? [spx.Parameter]
            : px is global::Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax pplx ? [..pplx.ParameterList.Parameters]
            : [];

        // We can replace the hardcoded Func type once we re-write this to use a method template which can be generic
        global::Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.RoslynAsExpressionReplacement.RoslynExpression<global::System.Func<string, bool>> x = new global::Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.RoslynAsExpressionReplacement.RoslynExpression<global::System.Func<string, bool>> {
            Parameters = parameters,
            Statements = statements
        };


        """""""""""",
        """"""""""""
        <>SM:PxThis
        """""""""""",
        """"""""""""
        .ReceiveExpression(x);

        """"""""""""
    ];

    public static QtMethod<CompileTimeUnknown> Intercept<TTemplateReferences>(
        in this QtClass @this,
        InvocationExpressionSyntax invocationToProxy,
        TTemplateReferences references,
        DeclareQtInterceptorVoidMethodWithRefs<TTemplateReferences> declaration
    ) {
        var sourceCode = new __DynamicallyBoundSourceCode {
            CodeTemplate = CachedSourceCodeTemplate_Intercept_99b72c1c902541378198fd3c39e2f9f1
        };

        var dynamicComponentBinders = new QtDynamicComponentBinderCollection {
            { "PxDynamicComponent:QtArgState:Lambda", new QtArgStateBinder_Lambda(Unsafe.As<TTemplateReferences, QtArgStateAccessor>(ref references).Lambda) }
        };
        return @this.__BindDynamicTemplateInterceptMethod(
            invocationToProxy,
            sourceCode,
            dynamicComponentBinders
        );
    }
}