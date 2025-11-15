using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.TwoStageBuilders.Components;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;
using Mumei.Roslyn.Common;

namespace Mumei.CodeGen.Qt;

/// <summary>
/// Rewrites any block syntax and generates a code block that can render
/// the provided block syntax with all it's dependencies resolved.
/// Type {
///   var x = 10;
///   Console.WriteLine(x);
///   var y = new TSomeDependency();
///   y.DoSomething();
///   return y;
/// }
///
/// (IRenderTreeBuilder builder, {Context}, {Inputs}) {
///    builder.Text("var x = 10;");
///    builder.Line();
///    builder.Text("global::System.Console.WriteLine(x);");
///    builder.Line();
///    builder.Text("var y = new ");
///    builder.Text(Inputs.TSomeInput.FullName);
///    builder.Text("();");
///    builder.Line();
///    builder.Text("y.DoSomething();");
///    builder.Line();
///    builder.Text("return ");
///    builder.Text(SomeInputRenderMacros.Render__Type(y));
/// }
///
/// </summary>
/// <param name="sm"></param>
internal sealed class SyntheticRenderCodeBlockSyntaxRewriter(
    SemanticModel sm,
    ISyntheticCodeBlockResolutionContext resolutionContext
) : GloballyQualifyingSyntaxRewriter(sm) {
    private SourceFileRenderTreeBuilder _renderTree = new();
    private readonly string _renderTreeExpression = resolutionContext.RenderTreeExpression();

    public static ISyntheticCodeBlock CreateSyntheticRenderBlock(
        SyntaxNode blockOrExpression,
        ITypeSymbol returnType,
        SemanticModel sm,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        if (blockOrExpression is StatementSyntax blockLikeSyntax) {
            return CreateSyntheticRenderBlockForBlock(blockLikeSyntax, sm, resolutionContext);
        }

        if (blockOrExpression is ExpressionSyntax expressionSyntax) {
            return CreateSyntheticRenderBlockForExpressionBody(expressionSyntax, returnType, sm, resolutionContext);
        }

        throw new InvalidOperationException("Expected either a BlockSyntax or an ExpressionSyntax");
    }

    private static ISyntheticCodeBlock CreateSyntheticRenderBlockForExpressionBody(
        ExpressionSyntax expressionBody,
        ITypeSymbol returnType,
        SemanticModel sm,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        if (sm.GetTypeInfo(expressionBody).Type is not { } expressionType) {
            throw new InvalidOperationException($"Could not determine the type of the expression {expressionBody}.");
        }

        var visitor = new SyntheticRenderCodeBlockSyntaxRewriter(sm, resolutionContext);
        var isVoidExpression = SymbolEqualityComparer.Default.Equals(sm.Compilation.GetSpecialType(SpecialType.System_Void), returnType);
        var body = visitor.Visit(expressionBody);

        visitor.EmitRenderText(isVoidExpression ? $"{body};" : $"return {body};");

        var syntheticCodeBlock = new SyntheticCodeBlock(visitor._renderTree.GetSourceText());
        return syntheticCodeBlock;
    }

    private static ISyntheticCodeBlock CreateSyntheticRenderBlockForBlock(
        SyntaxNode blockLikeSyntaxNode,
        SemanticModel sm,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        var visitor = new SyntheticRenderCodeBlockSyntaxRewriter(sm, resolutionContext);
        visitor.Visit(blockLikeSyntaxNode);
        var syntheticCodeBlock = new SyntheticCodeBlock(visitor._renderTree.GetSourceText());
        return syntheticCodeBlock;
    }

    public override SyntaxNode? VisitDoStatement(DoStatementSyntax node) {
        var updatedStatement = base.VisitDoStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node) {
        var updatedStatement = base.VisitExpressionStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node) {
        var updatedStatement = base.VisitLocalDeclarationStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node) {
        var updatedStatement = base.VisitReturnStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitIfStatement(IfStatementSyntax node) {
        var updatedStatement = base.VisitIfStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitForStatement(ForStatementSyntax node) {
        var updatedStatement = base.VisitForStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitGotoStatement(GotoStatementSyntax node) {
        var updatedStatement = base.VisitGotoStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitLockStatement(LockStatementSyntax node) {
        var updatedStatement = base.VisitLockStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitBreakStatement(BreakStatementSyntax node) {
        var updatedStatement = base.VisitBreakStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitCheckedStatement(CheckedStatementSyntax node) {
        var updatedStatement = base.VisitCheckedStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitContinueStatement(ContinueStatementSyntax node) {
        var updatedStatement = base.VisitContinueStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitEmptyStatement(EmptyStatementSyntax node) {
        var updatedStatement = base.VisitEmptyStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitFixedStatement(FixedStatementSyntax node) {
        var updatedStatement = base.VisitFixedStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitGlobalStatement(GlobalStatementSyntax node) {
        var updatedStatement = base.VisitGlobalStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitLabeledStatement(LabeledStatementSyntax node) {
        var updatedStatement = base.VisitLabeledStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitSwitchStatement(SwitchStatementSyntax node) {
        var updatedStatement = base.VisitSwitchStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitThrowStatement(ThrowStatementSyntax node) {
        var updatedStatement = base.VisitThrowStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitTryStatement(TryStatementSyntax node) {
        var updatedStatement = base.VisitTryStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitUnsafeStatement(UnsafeStatementSyntax node) {
        var updatedStatement = base.VisitUnsafeStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitUsingStatement(UsingStatementSyntax node) {
        var updatedStatement = base.VisitUsingStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node) {
        var updatedStatement = base.VisitWhileStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitYieldStatement(YieldStatementSyntax node) {
        var updatedStatement = base.VisitYieldStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node) {
        // Handle CompileTimeForEach here
        var updatedStatement = base.VisitForEachStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node) {
        var updatedStatement = base.VisitLocalFunctionStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitForEachVariableStatement(ForEachVariableStatementSyntax node) {
        var updatedStatement = base.VisitForEachVariableStatement(node);
        return updatedStatement;
    }

    private void EmitRenderText(string text) {
        _renderTree.Interpolate($"{_renderTreeExpression}.{nameof(IRenderTreeBuilder.Text)}(\"{text}\");");
        _renderTree.NewLine();
    }
}

// Synthetic symbol binding:
// var arg = invocation.Invocation.ArgumentList.Arguments[0].Expression;
// var syntheticMacroInvocation = SyntaxFactory.InvocationExpression(
//     SyntaxFactory.MemberAccessExpression(
//         SyntaxKind.SimpleMemberAccessExpression,
//         arg,
//         SyntaxFactory.IdentifierName("Bind__self")
//     )
// );
//
// var macroInvocation = context.SemanticModel.GetSpeculativeSymbolInfo(arg.SpanStart, syntheticMacroInvocation, SpeculativeBindingOption.BindAsExpression);
//     if (macroInvocation.Symbol is not IMethodSymbol macroBindingMethod) {
//     return IntermediateNode.None;
// }
//
// var __ = macroBindingMethod;

internal sealed class SyntheticCodeBlock(string block) : ISyntheticCodeBlock, ISyntheticConstructable<CodeBlockFragment> {
    // Since we do have access to the declaration site of the original code block
    // we could keep an actual syntax tree here and map that to the position of the
    // original code block to give the user access to speculative type information and
    // additional introspection options for the code block.
    public CodeBlockFragment Construct() {
        return new CodeBlockFragment(block);
    }
}

internal interface ISyntheticCodeBlockResolutionContext {
    public bool TryResolveIdentifier(SimpleNameSyntax identifier, [NotNullWhen(true)] out SyntaxNode? resolvedNode);
    public bool TryResolveMemberAccess(MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out SyntaxNode? resolvedNode);
    public bool TryResolveInvocation(InvocationExpressionSyntax invocation, [NotNullWhen(true)] out SyntaxNode? resolvedNode);

    public string RenderTreeExpression();
}

public sealed class SyntheticCodeBlockFromLambdaResolutionContext(SemanticModel sm, string renderTreeArgumentName = "renderTree") : ISyntheticCodeBlockResolutionContext {
    public bool TryResolveIdentifier(SimpleNameSyntax identifier, out SyntaxNode? resolvedNode) {
        resolvedNode = null;
        return false;
    }

    public bool TryResolveMemberAccess(MemberAccessExpressionSyntax memberAccess, out SyntaxNode? resolvedNode) {
        resolvedNode = null;
        return false;
    }

    public bool TryResolveInvocation(InvocationExpressionSyntax invocation, out SyntaxNode? resolvedNode) {
        resolvedNode = null;
        return false;
    }

    public string RenderTreeExpression() {
        return renderTreeArgumentName;
    }
}

public sealed class SyntheticCodeBlockFromLambdaWithInputsResolutionContext(
    SemanticModel sm,
    string renderTreeArgumentName = "λ__renderTree"
) : ISyntheticCodeBlockResolutionContext {
    public bool TryResolveIdentifier(SimpleNameSyntax identifier, out SyntaxNode? resolvedNode) {
        resolvedNode = null;
        return false;
    }

    public bool TryResolveMemberAccess(MemberAccessExpressionSyntax memberAccess, out SyntaxNode? resolvedNode) {
        resolvedNode = null;
        return false;
    }

    public bool TryResolveInvocation(InvocationExpressionSyntax invocation, out SyntaxNode? resolvedNode) {
        resolvedNode = null;
        return false;
    }

    public string RenderTreeExpression() {
        return renderTreeArgumentName;
    }
}