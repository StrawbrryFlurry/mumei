using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Roslyn;

#pragma warning disable
namespace Mumei.CodeGen.Playground;

file static class __QtInterceptorImpl_Renderer {
    private sealed class ContextBindablesAccessor_Example {
        public LambdaExpressionSyntax Lambda { get; }
    }

    internal interface IRenderer {
        public void Text(string s);

        public void Interpolate();

        [Obsolete("Replace with the renderer API")]
        public void Bind<TBindable>(TBindable bindable) where TBindable : IQtTemplateBindable;

        public void Node<TNode>(TNode node) where TNode : SyntaxNode;

        public void Child<TRenderable>(TRenderable renderable) where TRenderable : IRenderable;

        public void RequireFeature(IFeature feature);

        internal interface IFeature { }

        delegate void Render(IRenderer renderer);
    }

    internal sealed class ProxyMethodRenderContext<TState>(InvocationExpressionSyntax proxyInvocation, TState state) {
        public TState State { get; } = state;

        public MethodInfoRenderable MethodInfo() {
            return default;
        }

        public ThisRenderable This() {
            return default;
        }

        public readonly record struct MethodInfoRenderable : IRenderable {
            public void Render(IRenderer renderer) { }
        }

        public readonly record struct ThisRenderable : IRenderable {
            public void Render(IRenderer renderer) { }
        }
    }

    private readonly struct Intercept__MethodBodyTemplate_Example(ProxyMethodRenderContext<ContextBindablesAccessor_Example> ctx) : IRenderable {
        public void Render(IRenderer renderer) {
            renderer.Text(
                """"""""""""
                global::Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax px = (global::Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax) global::Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression(global::Microsoft.CodeAnalysis.SyntaxNodeExtensions.NormalizeWhitespace(
                """"""""""""
            );
            renderer.Node(ctx.State.Lambda);
            renderer.Text(
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
            renderer.Child(ctx.This());
            renderer.Text(
                """"""""""""
                .ReceiveExpression(x);

                """"""""""""
            );
        }
    };

    internal interface IRenderable {
        public void Render(IRenderer renderer);
    }

    public static QtMethod<CompileTimeUnknown> Intercept<TTemplateReferences>(
        in this QtClass @this,
        InvocationExpressionSyntax invocationToProxy,
        TTemplateReferences references,
        DeclareQtInterceptorVoidMethodWithRefs<TTemplateReferences> declaration
    ) {
        var state = Unsafe.As<TTemplateReferences, ContextBindablesAccessor_Example>(ref references);
        var bindingContext = new ProxyMethodRenderContext<ContextBindablesAccessor_Example>(invocationToProxy, state);
        var template = new Intercept__MethodBodyTemplate_Example(bindingContext);

        return @this.__BindDynamicTemplateInterceptMethod(
            invocationToProxy,
            (__DynamicallyBoundSourceCode) (dynamic) (IRenderer.Render) (dynamic) template
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