using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.DeclarationGenerator;

public sealed partial class DeclarationDefinitionGenerator {
    private static ImmutableArray<RenderFragment> CollectOutputMemberBindingExpressions(
        ICodeGenerationContext ctx,
        INamedTypeSymbol classDefinitionSymbol,
        ExpressionFragment classBuilder,
        ExpressionFragment resolveDynamicallyBoundType
    ) {
        var bindingExpressions = new ArrayBuilder<RenderFragment>();
        var outputMembers = new ArrayBuilder<ISymbol>();
        var inputMembers = new ArrayBuilder<ISymbol>();

        CollectOutputAndInputMembers(ctx, classDefinitionSymbol, ref outputMembers, ref inputMembers);

        var inputMemberNames = new HashSet<string>();
        foreach (var inputMember in inputMembers) {
            inputMemberNames.Add(inputMember.Name);
        }

        foreach (var member in outputMembers) {
            if (member is IFieldSymbol field) {
                var bindingExpression = MakeFieldOutputMemberBindingExpression(field, classBuilder, resolveDynamicallyBoundType);
                bindingExpressions.Add(bindingExpression);
                continue;
            }

            if (member is IPropertySymbol property) {
                var bindingExpression = MakePropertyOutputMemberBindingExpression(property, classBuilder, resolveDynamicallyBoundType);
                bindingExpressions.Add(bindingExpression);
                continue;
            }

            if (member is IMethodSymbol method) {
                var sm = ctx.Compilation.GetSemanticModel(method.DeclaringSyntaxReferences[0].SyntaxTree);
                var bindingExpression = MakeMethodOutputMemberBindingExpression(method, classBuilder, resolveDynamicallyBoundType, sm, inputMemberNames);
                bindingExpressions.Add(bindingExpression);
                continue;
            }
        }

        return bindingExpressions.ToImmutableArrayAndFree();
    }

    private static void CollectOutputAndInputMembers(
        ICodeGenerationContext ctx,
        INamedTypeSymbol classDefinitionSymbol,
        ref ArrayBuilder<ISymbol> outputMembers,
        ref ArrayBuilder<ISymbol> inputMembers
    ) {
        var outputAttribute = ctx.TypeFromCompilation<OutputAttribute>();
        var inputAttribute = ctx.TypeFromCompilation<InputAttribute>();

        foreach (var member in classDefinitionSymbol.GetMembers()) {
            if (member.HasAttribute(outputAttribute)) {
                outputMembers.Add(member);
            }

            if (member.HasAttribute(inputAttribute)) {
                inputMembers.Add(member);
            }
        }
    }


    private static RenderFragment MakePropertyOutputMemberBindingExpression(
        IPropertySymbol property,
        ExpressionFragment classBuilder,
        ExpressionFragment resolveDynamicallyBoundType
    ) {
        var type = property.Type is ITypeParameterSymbol ? new TypeInfoFragment(typeof(CompileTimeUnknown)) : property.Type.ToRenderFragment();

        return renderTree => {
            renderTree.InterpolatedLine($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.DeclareProperty)}<{type.FullName}>(");
            renderTree.StartBlock();
            WritePossiblyLateBoundGetTypeExpression(property.Type, classBuilder, resolveDynamicallyBoundType, renderTree);
            renderTree.Line(",");
            renderTree.InterpolatedLine($"{property.Name:q},");
            renderTree.InterpolatedLine($"new {typeof(SyntheticPropertyAccessorList)}(");
            renderTree.StartBlock();
            // ToDo: Create these based of the declared syntax of the property.
            renderTree.InterpolatedLine($"{typeof(SyntheticPropertyAccessor)}.{nameof(SyntheticPropertyAccessor.SimpleGetter)},");
            renderTree.Line("null");
            renderTree.EndBlock();
            renderTree.Line(")");
            renderTree.EndBlock();
            renderTree.Text(")");
            var accessibility = property.DeclaredAccessibility.ToAccessModifiers();
            renderTree.InterpolatedLine($".{nameof(ISyntheticPropertyBuilder<>.WithAccessibility)}({accessibility.RenderAsExpression});");
        };
    }

    private static RenderFragment MakeFieldOutputMemberBindingExpression(
        IFieldSymbol field,
        ExpressionFragment classBuilder,
        ExpressionFragment resolveDynamicallyBoundType
    ) {
        var type = field.Type is ITypeParameterSymbol ? new TypeInfoFragment(typeof(CompileTimeUnknown)) : field.Type.ToRenderFragment();

        return renderTree => {
            renderTree.InterpolatedLine($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.DeclareField)}<{type.FullName}>(");
            renderTree.StartBlock();
            WritePossiblyLateBoundGetTypeExpression(field.Type, classBuilder, resolveDynamicallyBoundType, renderTree);
            renderTree.Line(",");
            renderTree.InterpolatedLine($"{field.Name:q}");
            renderTree.EndBlock();
            renderTree.Text(")");

            var accessibility = field.DeclaredAccessibility.ToAccessModifiers();
            renderTree.InterpolatedLine($".{nameof(ISyntheticFieldBuilder<>.WithAccessibility)}({accessibility.RenderAsExpression});");
        };
    }

    private static RenderFragment MakeMethodOutputMemberBindingExpression(
        IMethodSymbol method,
        ExpressionFragment classBuilder,
        ExpressionFragment resolveDynamicallyBoundType,
        SemanticModel sm,
        HashSet<string> inputMemberNames
    ) {
        if (method.DeclaringSyntaxReferences is not [var declarationRef]) {
            throw new NotSupportedException();
        }

        if (declarationRef.GetSyntax() is not MethodDeclarationSyntax methodDeclarationSyntax) {
            throw new NotSupportedException();
        }


        return renderTree => {
            renderTree.Interpolate($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.DeclareMethod)}");
            renderTree.Interpolate($"<global::System.Delegate>");
            renderTree.InterpolatedLine($"({method.Name:q})");
            renderTree.StartBlock();

            var accessibility = methodDeclarationSyntax.Modifiers.ToAccessModifiers();
            renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithAccessibility)}({accessibility.RenderAsExpression})");
            renderTree.Interpolate($".{nameof(ISyntheticMethodBuilder<>.WithReturnType)}(");
            WritePossiblyLateBoundGetTypeExpression(method.ReturnType, classBuilder, resolveDynamicallyBoundType, renderTree);
            renderTree.Line(")");

            if (method.Parameters.Length != 0) {
                renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithParameters)}(");
                renderTree.StartBlock();
                for (var i = 0; i < method.Parameters.Length; i++) {
                    var methodParameter = method.Parameters[i];
                    renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ICodeGenerationContext.Parameter)}(");
                    renderTree.StartBlock();
                    WritePossiblyLateBoundGetTypeExpression(methodParameter.Type, classBuilder, resolveDynamicallyBoundType, renderTree);
                    renderTree.Line(",");
                    renderTree.InterpolatedLine($"{methodParameter.Name:q}");
                    renderTree.EndBlock();
                    renderTree.Line(")");
                    if (i < method.Parameters.Length - 1) {
                        renderTree.Line(",");
                    }
                }

                renderTree.EndBlock();
                renderTree.Line(")");
            }

            if (method.TypeParameters.Length != 0) {
                renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithTypeParameters)}(");
                renderTree.StartBlock();

                for (var i = 0; i < method.TypeParameters.Length; i++) {
                    var typeParameter = method.TypeParameters[i];
                    renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ICodeGenerationContext.TypeParameter)}(");
                    renderTree.StartBlock();
                    renderTree.InterpolatedLine($"{typeParameter.Name:q}");
                    renderTree.EndBlock();
                    renderTree.Text(")");
                    if (i < method.Parameters.Length - 1) {
                        renderTree.Line(",");
                    }
                }

                renderTree.EndBlock();
                renderTree.Line(")");
            }

            if (method.IsAbstract) {
                renderTree.Line(";");
                renderTree.EndBlock();
                return;
            }

            renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithBody)}(");
            renderTree.StartBlock();
            renderTree.InterpolatedLine($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ICodeGenerationContext.Block)}(");
            renderTree.StartBlock();
            renderTree.Line("this,");
            renderTree.InterpolatedLine($"static ({SyntheticDefinitionMethodCodeBlockResolutionContext.RenderTreeParameter}, {SyntheticDefinitionMethodCodeBlockResolutionContext.InputArgumentName}) => {{");
            renderTree.StartBlock();
            Debug.Assert(methodDeclarationSyntax.Body is not null);
            var renderBody = SyntheticRenderCodeBlockSyntaxRewriter.CreateSyntheticRenderBlock(methodDeclarationSyntax.Body, method.ReturnType, sm, new SyntheticClassDefinitionMethodCodeBlockResolutionContext(sm, inputMemberNames));
            renderTree.Block(renderBody);
            renderTree.NewLine();
            renderTree.EndBlock();
            renderTree.Line("}");
            renderTree.EndBlock();
            renderTree.Line(")");
            renderTree.EndBlock();
            renderTree.Line(");");
            renderTree.EndBlock();
        };
    }

    private static void WritePossiblyLateBoundGetTypeExpression(
        ITypeSymbol type,
        ExpressionFragment classBuilder,
        ExpressionFragment resolveDynamicallyBoundType,
        IRenderTreeBuilder renderTreeBuilder
    ) {
        if (type is not ITypeParameterSymbol { DeclaringMethod: null } typeParameter) {
            renderTreeBuilder.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context.Type)}({type.ToRenderFragment().TypeOf})");
            return;
        }

        renderTreeBuilder.Interpolate($"{resolveDynamicallyBoundType}(nameof({typeParameter.Name}))");
    }
}