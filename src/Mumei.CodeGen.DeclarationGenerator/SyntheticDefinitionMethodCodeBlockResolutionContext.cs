using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen.DeclarationGenerator;

internal sealed class SyntheticInterceptorMethodDefinitionMethodCodeBlockResolutionContext(
    SemanticModel sm,
    HashSet<string> inputMemberNames
) : SyntheticDefinitionMethodCodeBlockResolutionContext(sm, inputMemberNames) { }

internal sealed class SyntheticMethodDefinitionMethodCodeBlockResolutionContext(
    SemanticModel sm,
    HashSet<string> inputMemberNames
) : SyntheticDefinitionMethodCodeBlockResolutionContext(sm, inputMemberNames) { }

internal sealed class SyntheticClassDefinitionMethodCodeBlockResolutionContext(
    SemanticModel sm,
    HashSet<string> inputMemberNames
) : SyntheticDefinitionMethodCodeBlockResolutionContext(sm, inputMemberNames) { }

internal abstract class SyntheticDefinitionMethodCodeBlockResolutionContext(
    SemanticModel sm,
    HashSet<string> inputMemberNames
) : ISyntheticCodeBlockResolutionContext {
    public const string RenderTreeParameter = $"{Strings.PrivateLocal}renderTree";
    public const string InputArgumentName = $"{Strings.PrivateLocal}ctx";

    public SemanticModel SemanticModel => sm;

    public bool TryResolveThisExpression(MacroRenderTreeBuilder macroRenderContext, ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        // If this is not a member access to an input member we don't care about it since `this` will be valid in the generated context.
        slot = null;
        return false;
    }

    public bool TryUpdateThisExpression(ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveName(MacroRenderTreeBuilder macroRenderContext, SimpleNameSyntax identifier, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        if (!inputMemberNames.Contains(identifier.Identifier.Text)) {
            goto NotFound;
        }

        var memberOperation = sm.GetOperation(identifier);
        if (memberOperation is not IMemberReferenceOperation memberReference) {
            goto NotFound;
        }

        // Replace Member with ctx.Member
        var synthesizedMemberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(InputArgumentName),
            identifier
        );
        EmitInputMemberRenderExpression(macroRenderContext, identifier, synthesizedMemberAccess, memberReference.Member, out slot);
        return true;

        NotFound:
        slot = null;
        return false;
    }

    public bool TryUpdateName(SimpleNameSyntax identifier, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveMemberAccess(MacroRenderTreeBuilder macroRenderContext, MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        var targetMemberAccess = GetInnerMostMemberAccess(memberAccess);
        if (targetMemberAccess.Expression is not ThisExpressionSyntax || !inputMemberNames.Contains(targetMemberAccess.Name.Identifier.Text)) {
            goto NotFound;
        }

        var memberOperation = sm.GetOperation(memberAccess);
        if (memberOperation is not IMemberReferenceOperation memberReference) {
            goto NotFound;
        }

        // Replace this.Member with ctx.Member
        var synthesizedMemberAccess = memberAccess.ReplaceNode(targetMemberAccess.Expression, SyntaxFactory.IdentifierName(InputArgumentName));
        EmitInputMemberRenderExpression(macroRenderContext, memberAccess, synthesizedMemberAccess, memberReference.Member, out slot);
        return true;

        NotFound:
        slot = null;
        return false;
    }

    private void EmitInputMemberRenderExpression(
        MacroRenderTreeBuilder macroRenderContext,
        ExpressionSyntax expression,
        ExpressionSyntax synthesizedExpression,
        ISymbol memberSymbol,
        out MacroRenderTreeBuilder.MacroSlot slot
    ) {
        ITypeSymbol memberType;
        if (memberSymbol is IPropertySymbol propertySymbol) {
            memberType = propertySymbol.Type;
        } else if (memberSymbol is IFieldSymbol fieldSymbol) {
            memberType = fieldSymbol.Type;
        } else if (memberSymbol is IMethodSymbol methodSymbol) {
            memberType = methodSymbol.ReturnType;
        } else {
            slot = macroRenderContext.DeclareSlot(renderTree => {
                renderTree.Text($"#error Failed to generate a render expression for {expression} since {memberSymbol} is not a valid renderable member.");
            });
            return;
        }

        if (TryEmitRenderExpressionForType(macroRenderContext, expression, synthesizedExpression, memberType, out slot)) {
            return;
        }

        slot = macroRenderContext.DeclareSlot(renderTree => {
            renderTree.Text(
                $"#warning Failed to generate a render expression for {expression} since it's type '{memberType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}' can only be used in synthetic code blocks if it implements '{nameof(IRenderFragment)}', or if it has a suitable '{nameof(DefaultRenderExpressionExtensions.RenderExpression)}' method that takes a {nameof(IRenderTreeBuilder)} as its only parameter."
            );
        });
    }

    private bool TryEmitRenderExpressionForType(
        MacroRenderTreeBuilder macroRenderContext,
        ExpressionSyntax expressionToRender,
        ExpressionSyntax synthesizedExpressionToRender,
        ITypeSymbol typeSymbol,
        [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot
    ) {
        var renderFragmentType = sm.Compilation.GetTypeByMetadataName(typeof(IRenderFragment).FullName!)!;
        if (typeSymbol.AllInterfaces.Contains(renderFragmentType, SymbolEqualityComparer.Default)) {
            slot = macroRenderContext.DeclareSlot(renderTree => {
                renderTree.Interpolate($"{synthesizedExpressionToRender.NormalizeWhitespace().ToFullString()}.{nameof(IRenderFragment.Render)}({RenderTreeExpression()});");
            });
            return true;
        }

        var syntheticRenderInvocationExpression = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                expressionToRender,
                SyntaxFactory.IdentifierName(nameof(DefaultRenderExpressionExtensions.RenderExpression))
            ),
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(
                        SyntaxFactory.DefaultExpression(SyntaxFactory.IdentifierName(RuntimeTypeSerializer.GetTypeFullName(typeof(IRenderTreeBuilder)))
                        )
                    )
                )
            )
        );

        var syntheticRenderInvocationInformation = sm.GetSpeculativeSymbolInfo(
            expressionToRender.SpanStart,
            syntheticRenderInvocationExpression,
            SpeculativeBindingOption.BindAsExpression
        );

        if (syntheticRenderInvocationInformation.CandidateSymbols is not [IMethodSymbol renderToMethodSymbol]) {
            slot = null;
            return false;
        }

        if (!renderToMethodSymbol.IsExtensionMethod) {
            slot = macroRenderContext.DeclareSlot(renderTree => {
                renderTree.Interpolate($"{synthesizedExpressionToRender.NormalizeWhitespace().ToFullString()}.{renderToMethodSymbol.Name}({RenderTreeExpression()});");
            });
            return true;
        }

        slot = macroRenderContext.DeclareSlot(renderTree => {
            renderTree.Interpolate($"{renderToMethodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{renderToMethodSymbol.Name}({synthesizedExpressionToRender.NormalizeWhitespace().ToFullString()}, {RenderTreeExpression()});");
        });
        return true;
    }

    private static MemberAccessExpressionSyntax GetInnerMostMemberAccess(MemberAccessExpressionSyntax memberAccess) {
        var innerMostMemberAccess = memberAccess;
        var nextExpression = memberAccess.Expression;
        while (nextExpression is MemberAccessExpressionSyntax innerMemberAccess) {
            nextExpression = innerMemberAccess.Expression;
            innerMostMemberAccess = innerMemberAccess;
        }

        return innerMostMemberAccess;
    }

    public bool TryUpdateMemberAccess(MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveInvocation(MacroRenderTreeBuilder macroRenderContext, InvocationExpressionSyntax invocation, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        ExpressionSyntax synthesizedInvocationExpression;
        if (invocation.Expression is not MemberAccessExpressionSyntax && invocation.Expression is not SimpleNameSyntax) {
            goto NotFound;
        }

        if (invocation.Expression is MemberAccessExpressionSyntax potentialInputAccess) {
            var memberAccess = GetInnerMostMemberAccess(potentialInputAccess);
            if (memberAccess.Expression is not ThisExpressionSyntax || !inputMemberNames.Contains(memberAccess.Name.Identifier.Text)) {
                goto NotFound;
            }

            synthesizedInvocationExpression = invocation.ReplaceNode(memberAccess.Expression, SyntaxFactory.IdentifierName(InputArgumentName));
        } else {
            var invocationTarget = (SimpleNameSyntax) invocation.Expression;
            if (!inputMemberNames.Contains(invocationTarget.Identifier.Text)) {
                goto NotFound;
            }

            synthesizedInvocationExpression = invocation.WithExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(InputArgumentName),
                    invocationTarget
                )
            );
        }

        if (sm.GetOperation(invocation) is not IInvocationOperation invocationOperation) {
            goto NotFound;
        }

        EmitInputMemberRenderExpression(macroRenderContext, invocation, synthesizedInvocationExpression, invocationOperation.TargetMethod, out slot);
        return true;

        NotFound:
        slot = null;
        return false;
    }

    public bool TryUpdateInvocation(InvocationExpressionSyntax invocation, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string RenderTreeExpression() {
        return RenderTreeParameter;
    }
}