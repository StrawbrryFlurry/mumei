using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Roslyn.Common.Polyfill;

namespace Mumei.CodeGen.DeclarationGenerator;

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
/// </summary>
/// <param name="sm"></param>
internal sealed class SyntheticRenderCodeBlockSyntaxRewriter(
    ISyntheticCodeBlockResolutionContext resolutionContext
) : GloballyQualifyingSyntaxRewriter(resolutionContext.SemanticModel) {
    private MacroRenderTreeBuilder _renderTree = new(resolutionContext);

    public static string CreateSyntheticRenderBlock(
        SyntaxNode blockOrExpression,
        ITypeSymbol returnType,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        if (blockOrExpression is StatementSyntax blockLikeSyntax) {
            return CreateSyntheticRenderBlockForBlock(blockLikeSyntax, resolutionContext);
        }

        if (blockOrExpression is ExpressionSyntax expressionSyntax) {
            return CreateSyntheticRenderBlockForExpressionBody(expressionSyntax, returnType, resolutionContext);
        }

        throw new InvalidOperationException("Expected either a BlockSyntax or an ExpressionSyntax");
    }

    private static string CreateSyntheticRenderBlockForExpressionBody(
        ExpressionSyntax expressionBody,
        ITypeSymbol returnType,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        var visitor = new SyntheticRenderCodeBlockSyntaxRewriter(resolutionContext);
        var isVoidExpression = SymbolEqualityComparer.Default.Equals(resolutionContext.SemanticModel.Compilation.GetSpecialType(SpecialType.System_Void), returnType);
        var body = visitor.Visit(expressionBody);

        visitor._renderTree.EmitRenderSegment(isVoidExpression ? $"{body};" : $"return {body};");

        return visitor._renderTree.GetSourceText();
    }

    private static string CreateSyntheticRenderBlockForBlock(
        SyntaxNode blockLikeSyntaxNode,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        var visitor = new SyntheticRenderCodeBlockSyntaxRewriter(resolutionContext);
        var resolvedBlockLike = "";
        var node = visitor.Visit(blockLikeSyntaxNode);
        if (node is BlockSyntax blockLikeSyntax) {
            resolvedBlockLike = blockLikeSyntax.NormalizeWhitespace().Statements.ToFullString();
        } else {
            resolvedBlockLike = node.NormalizeWhitespace().ToFullString();
        }

        visitor._renderTree.EmitRenderSegment(resolvedBlockLike);
        return visitor._renderTree.GetSourceText();
    }

    public override SyntaxNode? VisitIfStatement(IfStatementSyntax node) {
        var updatedStatement = base.VisitIfStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitForStatement(ForStatementSyntax node) {
        var updatedStatement = base.VisitForStatement(node);
        return updatedStatement;
    }

    // Compile Time Switch?
    public override SyntaxNode? VisitSwitchStatement(SwitchStatementSyntax node) {
        var updatedStatement = base.VisitSwitchStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node) {
        var updatedStatement = base.VisitWhileStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node) {
        // Handle CompileTimeForEach here
        var updatedStatement = base.VisitForEachStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitForEachVariableStatement(ForEachVariableStatementSyntax node) {
        var updatedStatement = base.VisitForEachVariableStatement(node);
        return updatedStatement;
    }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node) {
        // Unless the invocation itself is a macro which we need to emit, we can simply delegate
        // replacing identifiers to the identifier visitor. Invoke needs to replace
        // the entire invocation, since we just need the identifier at this position
        SimpleNameSyntax methodName;

        if (resolutionContext.TryResolveInvocation(_renderTree, node, out var resolvedSlot)) {
            return resolvedSlot.SyntaxNode.WithTriviaFrom(node);
        }

        if (resolutionContext.TryUpdateInvocation(node, out var updatedNode)) {
            return updatedNode;
        }

        if (node.Expression is SimpleNameSyntax identifier) {
            methodName = identifier;
            goto TryResolveInvocationMacro;
        }

        if (node.Expression is MemberAccessExpressionSyntax memberAccess) {
            // if (memberAccess.Expression is not ThisExpressionSyntax) {
            //     return base.VisitInvocationExpression(node);
            // }

            methodName = memberAccess.Name;
            goto TryResolveInvocationMacro;
        }

        return base.VisitInvocationExpression(node);

        TryResolveInvocationMacro:
        return base.VisitInvocationExpression(node);
    }

    public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
        if (resolutionContext.TryResolveMemberAccess(_renderTree, node, out var slot)) {
            return slot.SyntaxNode.WithTriviaFrom(node);
        }

        if (resolutionContext.TryUpdateMemberAccess(node, out var updatedNode)) {
            return updatedNode;
        }

        return base.VisitMemberAccessExpression(node);
    }

    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
        if (resolutionContext.TryResolveName(_renderTree, node, out var slot)) {
            return slot.SyntaxNode.WithTriviaFrom(node);
        }

        if (resolutionContext.TryUpdateName(node, out var updatedNode)) {
            return updatedNode;
        }

        return base.VisitIdentifierName(node);
    }

    public override SyntaxNode? VisitGenericName(GenericNameSyntax node) {
        if (resolutionContext.TryResolveName(_renderTree, node, out var slot)) {
            return slot.SyntaxNode.WithTriviaFrom(node);
        }

        if (resolutionContext.TryUpdateName(node, out var updatedNode)) {
            return updatedNode;
        }

        return base.VisitGenericName(node);
    }

    public override SyntaxNode? VisitThisExpression(ThisExpressionSyntax node) {
        if (resolutionContext.TryResolveThisExpression(_renderTree, node, out var resolvedSlot)) {
            return resolvedSlot.SyntaxNode.WithTriviaFrom(node);
        }

        if (resolutionContext.TryUpdateThisExpression(node, out var updatedNode)) {
            return updatedNode;
        }

        return base.VisitThisExpression(node);
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

internal sealed class QtSyntheticCodeBlock(string block) : ISyntheticCodeBlock, ISyntheticConstructable<CodeBlockFragment> {
    // Since we do have access to the declaration site of the original code block
    // we could keep an actual syntax tree here and map that to the position of the
    // original code block to give the user access to speculative type information and
    // additional introspection options for the code block.
    public CodeBlockFragment Construct() {
        return new CodeBlockFragment(block);
    }

    public CodeBlockFragment Construct(ICompilationUnitContext compilationUnit) {
        return new CodeBlockFragment(block);
    }
}

internal interface ISyntheticCodeBlockResolutionContext {
    public SemanticModel SemanticModel { get; }

    public bool TryResolveThisExpression(MacroRenderTreeBuilder macroRenderContext, ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot);
    public bool TryUpdateThisExpression(ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out SyntaxNode? updatedNode);

    public bool TryResolveName(MacroRenderTreeBuilder macroRenderContext, SimpleNameSyntax identifier, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot);
    public bool TryUpdateName(SimpleNameSyntax identifier, [NotNullWhen(true)] out SyntaxNode? updatedNode);
    public bool TryResolveMemberAccess(MacroRenderTreeBuilder macroRenderContext, MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot);
    public bool TryUpdateMemberAccess(MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out SyntaxNode? updatedNode);
    public bool TryResolveInvocation(MacroRenderTreeBuilder macroRenderContext, InvocationExpressionSyntax invocation, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot);
    public bool TryUpdateInvocation(InvocationExpressionSyntax invocation, [NotNullWhen(true)] out SyntaxNode? updatedNode);

    public string RenderTreeExpression();
}

internal sealed class SyntheticCodeBlockFromLambdaResolutionContext(SemanticModel sm, string renderTreeArgumentName = "renderTree") : ISyntheticCodeBlockResolutionContext {
    public MacroRenderTreeBuilder MacroRenderContext { get; set; }
    public SemanticModel SemanticModel { get; } = sm;

    public bool TryResolveThisExpression(MacroRenderTreeBuilder macroRenderContext, ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        slot = null;
        return false;
    }

    public bool TryUpdateThisExpression(ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveName(MacroRenderTreeBuilder macroRenderContext, SimpleNameSyntax identifier, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        slot = null;
        return false;
    }

    public bool TryUpdateName(SimpleNameSyntax identifier, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveMemberAccess(MacroRenderTreeBuilder macroRenderContext, MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        slot = null;
        return false;
    }

    public bool TryUpdateMemberAccess(MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveInvocation(MacroRenderTreeBuilder macroRenderContext, InvocationExpressionSyntax invocation, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        slot = null;
        return false;
    }

    public bool TryUpdateInvocation(InvocationExpressionSyntax invocation, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public string RenderTreeExpression() {
        return renderTreeArgumentName;
    }
}

internal sealed class SyntheticCodeBlockFromLambdaWithInputsResolutionContext(
    SemanticModel sm,
    string inputArgumentName,
    string synthesizedInputArgumentName,
    string renderTreeArgumentName = "λ__renderTree"
) : ISyntheticCodeBlockResolutionContext {
    public SemanticModel SemanticModel { get; } = sm;
    public MacroRenderTreeBuilder MacroRenderContext { get; set; }

    public bool TryResolveThisExpression(MacroRenderTreeBuilder macroRenderContext, ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        // Lambdas do not have a "this" context they can refer to since we require them to be static
        slot = null;
        return false;
    }

    public bool TryUpdateThisExpression(ThisExpressionSyntax thisExpression, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveName(MacroRenderTreeBuilder macroRenderContext, SimpleNameSyntax identifier, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        slot = null;
        return false;
    }

    public bool TryUpdateName(SimpleNameSyntax identifier, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveMemberAccess(MacroRenderTreeBuilder macroRenderContext, MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        var target = GetInnerMostMemberAccessTarget(memberAccess);
        if (target is not SimpleNameSyntax name || name.Identifier.Text != inputArgumentName) {
            goto NotFound;
        }

        var memberOperation = sm.GetOperation(memberAccess);
        if (memberOperation is not IMemberReferenceOperation memberReference) {
            goto NotFound;
        }

        var synthesizedMemberAccess = memberAccess.ReplaceNode(target, SyntaxFactory.IdentifierName(synthesizedInputArgumentName));
        EmitInputMemberRenderExpression(memberAccess, synthesizedMemberAccess, memberReference.Member, out slot);
        return true;

        NotFound:
        slot = null;
        return false;
    }

    private void EmitInputMemberRenderExpression(
        ExpressionSyntax memberAccess,
        ExpressionSyntax synthesizedMemberAccess,
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
            slot = MacroRenderContext.DeclareSlot(renderTree => {
                renderTree.Text($"#error Failed to generate a render expression for {memberAccess} since {memberSymbol} is not a valid renderable member.");
            });
            return;
        }

        if (TryEmitRenderExpressionForType(memberAccess, synthesizedMemberAccess, memberType, out slot)) {
            return;
        }

        slot = MacroRenderContext.DeclareSlot(renderTree => {
            renderTree.Text(
                "#warning Failed to generate a render expression for {memberAccess} since it's type '{memberType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}' can only be used in synthetic code blocks if it implements '{nameof(IRenderFragment)}', or if it has a suitable '{nameof(DefaultRenderExpressionExtensions.RenderTo)}' method that takes a {nameof(IRenderTreeBuilder)} as its only parameter."
            );
        });
    }

    private bool TryEmitRenderExpressionForType(
        ExpressionSyntax expressionToRender,
        ExpressionSyntax synthesizedExpressionToRender,
        ITypeSymbol typeSymbol,
        [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot
    ) {
        var renderFragmentType = sm.Compilation.GetTypeByMetadataName(typeof(IRenderFragment).FullName!)!;
        if (typeSymbol.AllInterfaces.Contains(renderFragmentType, SymbolEqualityComparer.Default)) {
            slot = MacroRenderContext.DeclareSlot(renderTree => {
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
            slot = MacroRenderContext.DeclareSlot(renderTree => {
                renderTree.Interpolate($"{synthesizedExpressionToRender.NormalizeWhitespace().ToFullString()}.{renderToMethodSymbol.Name}({RenderTreeExpression()});");
            });
            return true;
        }

        slot = MacroRenderContext.DeclareSlot(renderTree => {
            renderTree.Interpolate($"{renderToMethodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{renderToMethodSymbol.Name}({synthesizedExpressionToRender.NormalizeWhitespace().ToFullString()}, {RenderTreeExpression()});");
        });
        return true;
    }

    private static ExpressionSyntax GetInnerMostMemberAccessTarget(MemberAccessExpressionSyntax memberAccess) {
        var targetExpression = memberAccess.Expression;
        while (targetExpression is MemberAccessExpressionSyntax innerMemberAccess) {
            targetExpression = innerMemberAccess.Expression;
        }

        return targetExpression;
    }

    public bool TryUpdateMemberAccess(MemberAccessExpressionSyntax memberAccess, [NotNullWhen(true)] out SyntaxNode? updatedNode) {
        updatedNode = null;
        return false;
    }

    public bool TryResolveInvocation(MacroRenderTreeBuilder macroRenderContext, InvocationExpressionSyntax invocation, [NotNullWhen(true)] out MacroRenderTreeBuilder.MacroSlot? slot) {
        if (invocation.Expression is not MemberAccessExpressionSyntax potentialInputAccess) {
            goto NotFound;
        }

        var target = GetInnerMostMemberAccessTarget(potentialInputAccess);
        if (target is not SimpleNameSyntax name || name.Identifier.Text != inputArgumentName) {
            goto NotFound;
        }

        if (sm.GetOperation(invocation) is not IInvocationOperation invocationOperation) {
            goto NotFound;
        }

        var synthesizedMemberAccess = invocation.ReplaceNode(target, SyntaxFactory.IdentifierName(synthesizedInputArgumentName));
        EmitInputMemberRenderExpression(invocation, synthesizedMemberAccess, invocationOperation.TargetMethod, out slot);
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
        return renderTreeArgumentName;
    }
}

internal sealed class MacroRenderTreeBuilder(ISyntheticCodeBlockResolutionContext rendererResolutionContext) {
    private static ReadOnlySpan<char> StartRenderMacroSlot => "<λRENDERMACROSLOT_";
    private static ReadOnlySpan<char> CloseRenderMacroSlot => "λ>";

    private readonly ImmutableArray<MacroSlot>.Builder _slots = ImmutableArray.CreateBuilder<MacroSlot>();
    private readonly string _renderTreeExpression = rendererResolutionContext.RenderTreeExpression();

    private readonly SourceFileRenderTreeBuilder _renderTree = new();

    public string GetSourceText() {
        return _renderTree.GetSourceText();
    }

    public MacroSlot DeclareSlot(RenderFragment renderFragment) {
        var slot = new MacroSlot(_slots.Count, renderFragment);
        _slots.Add(slot);
        return slot;
    }

    public void EmitRenderSegment(string segment) {
        var segmentSpan = segment.AsSpan();
        while (!segmentSpan.IsEmpty) {
            var nextSlotPos = segmentSpan.IndexOf(StartRenderMacroSlot, StringComparison.InvariantCulture);
            if (nextSlotPos == -1) {
                EmitRenderText(segmentSpan);
                break;
            }

            if (nextSlotPos > 0) {
                var textBeforeSlot = segmentSpan[..nextSlotPos];
                EmitRenderText(textBeforeSlot);
            }

            var slotStart = nextSlotPos + StartRenderMacroSlot.Length;
            var slotEnd = segmentSpan.IndexOf(CloseRenderMacroSlot, StringComparison.InvariantCulture);
            Debug.Assert(slotEnd != -1, "Unexpected end of render macro slot marker");

            var slotIdxStr = segmentSpan[slotStart..slotEnd];
            var couldParse = int.TryParseAsciiInt(slotIdxStr, out var slotIdx);
            Debug.Assert(slotIdx >= 0, "Render macro slot index must be non-negative");
            Debug.Assert(couldParse, "Couldn't parse render macro slot index");

            EmitRenderMacroSlot(slotIdx);

            var afterMacroClosePos = slotEnd + CloseRenderMacroSlot.Length;
            segmentSpan = segmentSpan[afterMacroClosePos..];
        }
    }

    private void EmitRenderText(ReadOnlySpan<char> text) {
        _renderTree.Interpolate($"{_renderTreeExpression}.{nameof(IRenderTreeBuilder.Text)}({Strings.RawStringLiteral12}");
        _renderTree.NewLine();
        _renderTree.Text(text);
        _renderTree.NewLine();
        _renderTree.Interpolate($"{Strings.RawStringLiteral12});");
        _renderTree.NewLine();
    }

    private void EmitRenderMacroSlot(int slotIdx) {
        Debug.Assert(slotIdx >= 0 && slotIdx < _slots.Count, "Invalid render macro slot index");
        var slot = _slots[slotIdx];
        slot.EmitRenderStatements(_renderTree);
        _renderTree.NewLine();
    }

    public sealed class MacroSlot(int idx, RenderFragment renderFragment) {
        public string Name => field ??= MakeMacroIdentifierName();
        public IdentifierNameSyntax SyntaxNode => SyntaxFactory.IdentifierName(Name);

        public void EmitRenderStatements(IRenderTreeBuilder builder) {
            renderFragment(builder);
        }

        private string MakeMacroIdentifierName() {
            var sb = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
            sb.Write(StartRenderMacroSlot);
            sb.WriteLiteral(idx);
            sb.Write(CloseRenderMacroSlot);
            return sb.ToString();
        }
    }
}