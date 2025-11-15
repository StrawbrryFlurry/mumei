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
        SemanticModel sm,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        var visitor = new SyntheticRenderCodeBlockSyntaxRewriter(sm, resolutionContext);
        visitor.Visit(blockOrExpression);
        var syntheticCodeBlock = new SyntheticCodeBlock(visitor._renderTree.GetSourceText());
        return syntheticCodeBlock;
    }

    private void EmitRenderText(string text) {
        _renderTree.Interpolate($"{_renderTreeExpression}.{nameof(IRenderTreeBuilder.Text)}({text});");
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
    string renderTreeArgumentName = "renderTree"
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