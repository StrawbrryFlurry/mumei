using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Roslyn;
using Mumei.Roslyn.Common;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Mumei.CodeGen.Qt;

public sealed partial class QtClassFactoryInterceptorGenerator {
    private void BindTemplateInterceptMethod(
        SyntaxWriter result,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        IInvocationOperation invocationOperation,
        SourceProductionContext context
    ) {
        var args = QtClassDynamicDeclarationExtensions.ParseAddTemplateInterceptMethodArguments(invocationOperation);
        var methodTemplateArg = invocation.ArgumentList.Arguments[1];
        if (args.Template.Value is not IObjectCreationOperation templateCreationOp) {
            Diagnostics.ReportMethodTemplateNotFromCallSite(context, methodTemplateArg);
            return;
        }

        if (templateCreationOp.Constructor is null) {
            throw new InvalidOperationException("Constructor of template factory reference is null.");
        }

        if (templateCreationOp.Constructor.DeclaringSyntaxReferences.Length == 0) {
            Diagnostics.ReportMethodTemplateNotDeclaredInCompilation(context, methodTemplateArg);
            return;
        }

        var constructorDeclaration = templateCreationOp.Constructor.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken);

        if (constructorDeclaration is not ClassDeclarationSyntax { ParameterList: { } primaryConstructor } templateClassDeclaration) {
            Diagnostics.ReportMethodTemplateUsesNonPrimaryConstructor(context, methodTemplateArg);
            return;
        }

        var templateType = templateCreationOp.Constructor.DeclaringType();
        var selectedMethod = GetMethodTemplateMethodFromSelection(args.MethodSelector, templateType, context);
        if (selectedMethod is null) {
            return;
        }

        var methodTemplateDeclaration = selectedMethod.DeclaringSyntaxReferences.Length == 0
            ? throw new NotSupportedException()
            : selectedMethod.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;


        var stateIdentifiers = primaryConstructor.Parameters.Select(x => x.Identifier.Text).ToImmutableHashSet();
        var visitor = new QtMethodTemplateDeclarationVisitor(semanticModel, stateIdentifiers);

        var updatedMethodDeclaration = visitor.Visit(methodTemplateDeclaration) as MethodDeclarationSyntax;
        var body = updatedMethodDeclaration.Body;
    }

    private IMethodSymbol? GetMethodTemplateMethodFromSelection(
        IArgumentOperation methodSelectionArg,
        ITypeSymbol declaringType,
        SourceProductionContext context
    ) {
        if (methodSelectionArg.Syntax is not ArgumentSyntax { Expression: LambdaExpressionSyntax lambda } argument) {
            throw new InvalidOperationException("Expected method selection argument to be a lambda expression.");
        }

        if (lambda.Body is not MemberAccessExpressionSyntax memberAccess) {
            Diagnostics.ReportRequireSimpleMethodTemplateMethodSelector(context, argument);
            return null;
        }

        var selectedMethodSymbol = methodSelectionArg.SemanticModel!.GetSymbolInfo(memberAccess.Name);
        if (selectedMethodSymbol.CandidateSymbols.Length > 1 && selectedMethodSymbol.CandidateReason == CandidateReason.OverloadResolutionFailure) {
            var bestCandidate = selectedMethodSymbol.CandidateSymbols.FirstOrDefault(x => x is IMethodSymbol ms && SymbolEqualityComparer.Default.Equals(ms.DeclaringType(), declaringType));
            if (bestCandidate is not null) {
                return (IMethodSymbol) bestCandidate;
            }
        }

        if (selectedMethodSymbol.Symbol is not IMethodSymbol methodSymbol) {
            Diagnostics.ReportMethodTemplateMethodSelectorSelectedNonTemplateMethod(context, argument);
            return null;
        }

        return methodSymbol;
    }

    private sealed class QtMethodTemplateDeclarationVisitor(
        SemanticModel sm,
        ImmutableHashSet<string> stateIdentifiers
    ) : GloballyQualifyingSyntaxRewriter(sm) {
        private const string _replaceStateExpressionWithAnnotationKind = "ReplaceStateExpressionWith";
        private const string _replaceContextExpressionWithAnnotationKind = "ReplaceContextExpressionWith";

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node) {
            if (node.Expression is not MemberAccessExpressionSyntax memberAccess) {
                return base.VisitInvocationExpression(node);
            }

            if (
                memberAccess.Name is GenericNameSyntax {
                    TypeArgumentList.Arguments.Count: 1,
                    Identifier.Text: nameof(IQtThis.Is)
                }
            ) {
                // Ignore all Is<> calls
                // If we called Is<> on <This> we still want to visit the <This>
                // expression to write the this binding into this place.
                // TODO: Should we track what casts are performed here and validate
                // that the the target type is a valid conversion for <this> when
                // the resulting template is bound?
                return Visit(memberAccess.Expression);
            }

            // This would be a call on the ctx object e.g. ctx.Invoke(...)
            if (memberAccess.Expression is SimpleNameSyntax invokee) {
                if (invokee.Identifier.Text != "this") {
                    return base.VisitInvocationExpression(node);
                }

                if (TryMakeMakerLiteralFor(memberAccess.Name.Identifier, node, out var result)) {
                    return result;
                }

                return base.VisitInvocationExpression(node);
            }

            // Since all state expressions are on the state object, invocations on the state object
            // will (almost) always be a nested member access e.g. state.someRef.Invoke();
            if (memberAccess.Expression is MemberAccessExpressionSyntax possibleStateAccess) {
                if (possibleStateAccess.Expression is not SimpleNameSyntax stateIdentifier) {
                    return base.VisitInvocationExpression(node);
                }

                if (stateIdentifier.Identifier.Text != "this") {
                    return base.VisitInvocationExpression(node);
                }

                // Not suuper sure if we can even do something here, since most state binding
                // is already handled by replacing the dynamic component binding - we'll see.
            }

            return base.VisitInvocationExpression(node);

        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
            if (stateIdentifiers.Contains(node.Identifier.Text)) {
                return MakeMakerLiteralCore(DynamicQtComponentBinder.CreateDynamicComponentBinderKey($"QtArgCtx:{node.Identifier.Text}"), node);
            }
            return base.VisitIdentifierName(node);
        }

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
            if (node.Expression is not SimpleNameSyntax identifier) {
                return base.VisitMemberAccessExpression(node);
            }

            var identifierName = identifier.Identifier.Text;
            if (identifierName != "this") {
                return base.VisitMemberAccessExpression(node);
            }

            if (TryMakeMakerLiteralFor(node.Name.Identifier, node, out var result)) {
                return result;
            }

            if (stateIdentifiers.Contains(identifierName)) {
                return MakeMakerLiteralCore(DynamicQtComponentBinder.CreateDynamicComponentBinderKey($"QtArgState:{node.Name.Identifier.Text}"), node);
            }

            return base.VisitMemberAccessExpression(node);
        }

        private bool TryMakeMakerLiteralFor(SyntaxToken identifier, SyntaxNode sourceNode, [NotNullWhen(true)] out IdentifierNameSyntax? result) {
            var binderKey = identifier.Text switch {
                nameof(QtDynamicInterceptorMethodCtx.This) => ProxyInvocationExpressionBindingContext.BindThis,
                nameof(QtDynamicInterceptorMethodCtx.Invoke) => ProxyInvocationExpressionBindingContext.BindInvocation,
                nameof(QtDynamicInterceptorMethodCtx.Method) => ProxyInvocationExpressionBindingContext.BindMethodInfo,
                nameof(QtDynamicInterceptorMethodCtx.InvocationArguments) => ProxyInvocationExpressionBindingContext.BindArguments,
                _ => null
            };

            if (binderKey is null) {
                result = null;
                return false;
            }

            result = MakeMakerLiteralCore(binderKey, sourceNode);
            return true;
        }

        private IdentifierNameSyntax MakeMakerLiteralCore(string markerExpr, SyntaxNode sourceNode) {
            return IdentifierName(
                    $"{__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeStart}{markerExpr}{__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeEnd}"
                ).WithLeadingTrivia(sourceNode.GetLeadingTrivia())
                .WithTrailingTrivia(sourceNode.GetTrailingTrivia());
        }
    }
}