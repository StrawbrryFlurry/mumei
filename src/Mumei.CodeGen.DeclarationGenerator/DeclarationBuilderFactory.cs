using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn;
using Mumei.CodeGen.Roslyn.Components;

namespace Mumei.CodeGen.DeclarationGenerator;

internal sealed class DeclarationBuilderFactory {
    public static RendererExpressionFragment DeclareFieldFromDefinitionMember(
        ExpressionFragment classBuilder,
        IFieldSymbol field
    ) {
        var type = GetTypeKnownAtCompileTime(field.Type);
        var getTypeExpression = MakeSyntheticTypeExpressionForType(field.Type);

        return DeclareField(
            classBuilder,
            type,
            getTypeExpression,
            field.Name,
            field.DeclaredAccessibility.ToAccessModifiers()
        );
    }

    public static RendererExpressionFragment DeclareField(
        ExpressionFragment classBuilder,
        TypeInfoFragment type,
        InvocationExpressionFragment getTypeExpression,
        string name,
        AccessModifierList accessibility
    ) {
        return RendererExpressionFragment.For(renderTree => {
            renderTree.InterpolatedLine($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.DeclareField)}<{type.FullName}>(");
            renderTree.StartBlock();
            renderTree.Node(getTypeExpression);
            renderTree.Line(",");
            renderTree.InterpolatedLine($"{name:q}");
            renderTree.EndBlock();
            renderTree.Text(")");
            renderTree.InterpolatedLine($".{nameof(ISyntheticFieldBuilder<>.WithAccessibility)}({accessibility.RenderAsExpression});");
        });
    }

    public static RendererExpressionFragment DeclarePropertyFromDefinitionMember(
        ExpressionFragment classBuilder,
        IPropertySymbol property
    ) {
        var type = GetTypeKnownAtCompileTime(property.Type);
        var getTypeExpression = MakeSyntheticTypeExpressionForType(property.Type);

        return DeclareProperty(
            classBuilder,
            type,
            getTypeExpression,
            property.Name,
            property.DeclaredAccessibility.ToAccessModifiers()
        );
    }

    public static RendererExpressionFragment DeclareProperty(
        ExpressionFragment classBuilder,
        TypeInfoFragment type,
        InvocationExpressionFragment getTypeExpression,
        string name,
        AccessModifierList accessibility
    ) {
        return RendererExpressionFragment.For(renderTree => {
            renderTree.InterpolatedLine($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.DeclareProperty)}<{type.FullName}>(");
            renderTree.StartBlock();
            renderTree.Node(getTypeExpression);
            renderTree.Line(",");
            renderTree.InterpolatedLine($"{name:q},");
            renderTree.InterpolatedLine($"new {typeof(SyntheticPropertyAccessorList)}(");
            renderTree.StartBlock();
            // ToDo: Create these based of the declared syntax of the property.
            renderTree.InterpolatedLine($"{typeof(SyntheticPropertyAccessor)}.{nameof(SyntheticPropertyAccessor.SimpleGetter)},");
            renderTree.Line("null");
            renderTree.EndBlock();
            renderTree.Line(")");
            renderTree.EndBlock();
            renderTree.Text(")");
            renderTree.InterpolatedLine($".{nameof(ISyntheticPropertyBuilder<>.WithAccessibility)}({accessibility.RenderAsExpression});");
        });
    }

    public static RendererExpressionFragment DeclareMethodFromDefinition(
        ExpressionFragment classBuilder,
        IMethodSymbol method,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        if (method.DeclaringSyntaxReferences is not [var declarationRef]) {
            throw new InvalidOperationException($"The method {method.ContainingType.Name}.{method.Name} was not declared in this compilation.");
        }

        if (declarationRef.GetSyntax() is not MethodDeclarationSyntax methodDeclarationSyntax) {
            throw new NotSupportedException();
        }

        var declareParameterList = MakeParameterListFromDefinition(classBuilder, method.Parameters);
        var declareTypeParameterList = MakeTypeParameterListFromDefinition(classBuilder, method.TypeParameters);
        var returnType = MakeSyntheticTypeExpressionForType(method.ReturnType);

        var declareBody = method.IsAbstract
            ? null
            : MakeCodeBlockFromMethodDeclaration(
                classBuilder,
                methodDeclarationSyntax,
                method.ReturnType,
                resolutionContext
            );

        return DeclareMethod(
            classBuilder,
            returnType,
            method.Name,
            method.DeclaredAccessibility.ToAccessModifiers(),
            declareTypeParameterList,
            declareParameterList,
            declareBody
        );
    }

    public static RendererExpressionFragment DeclareMethod(
        ExpressionFragment classBuilder,
        InvocationExpressionFragment getReturnTypeExpression,
        string name,
        AccessModifierList accessibility,
        RendererExpressionFragment? declareTypeParameterList,
        RendererExpressionFragment? declareParameterList,
        RendererExpressionFragment? declareBody
    ) {
        return RendererExpressionFragment.For(renderTree => {
            renderTree.Interpolate($"{classBuilder}.{nameof(ISyntheticClassBuilder<>.DeclareMethod)}");
            renderTree.Interpolate($"<{typeof(Delegate)}>");
            renderTree.InterpolatedLine($"({name:q})");

            renderTree.StartBlock();
            renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithAccessibility)}({accessibility.RenderAsExpression})");
            renderTree.Interpolate($".{nameof(ISyntheticMethodBuilder<>.WithReturnType)}(");
            renderTree.Node(getReturnTypeExpression);
            renderTree.Line(")");

            if (declareTypeParameterList is not null) {
                renderTree.Node(declareTypeParameterList);
            }

            if (declareParameterList is not null) {
                renderTree.Node(declareParameterList);
            }

            if (declareBody is null) {
                renderTree.Line(";");
                renderTree.EndBlock();
                return;
            }

            renderTree.Node(declareBody);
        });
    }

    public static RendererExpressionFragment? MakeCodeBlockFromMethodDeclaration(
        ExpressionFragment classBuilder,
        MethodDeclarationSyntax methodDeclarationSyntax,
        ITypeSymbol returnType,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        if (methodDeclarationSyntax.Body is null) {
            return null;
        }

        var renderBody = SyntheticRenderCodeBlockSyntaxRewriter.CreateSyntheticRenderBlock(
            methodDeclarationSyntax.Body,
            returnType,
            resolutionContext
        );

        return RendererExpressionFragment.For(renderTree => {
            renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithBody)}(");
            renderTree.StartBlock();
            renderTree.InterpolatedLine($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ICodeGenerationContext.Block)}(");
            renderTree.StartBlock();
            renderTree.Line("this,");
            renderTree.InterpolatedLine($"static ({SyntheticDefinitionMethodCodeBlockResolutionContext.RenderTreeParameter}, {SyntheticDefinitionMethodCodeBlockResolutionContext.InputArgumentName}) => {{");
            renderTree.StartBlock();

            renderTree.Block(renderBody);

            renderTree.NewLine();
            renderTree.EndBlock();
            renderTree.Line("}");
            renderTree.EndBlock();
            renderTree.Line(")");
            renderTree.EndBlock();
            renderTree.Line(");");
            renderTree.EndBlock();
        });
    }

    public static RendererExpressionFragment? MakeParameterListFromDefinition(
        ExpressionFragment classBuilder,
        ImmutableArray<IParameterSymbol> parameters
    ) {
        if (parameters.Length == 0) {
            return null;
        }

        return RendererExpressionFragment.For(renderTree => {
            renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithParameters)}(");
            renderTree.StartBlock();

            for (var i = 0; i < parameters.Length; i++) {
                var parameter = parameters[i];
                renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ICodeGenerationContext.Parameter)}(");
                renderTree.StartBlock();
                renderTree.Node(MakeSyntheticTypeExpressionForType(parameter.Type)); // TODO: This only works when used in synthetic definitions
                renderTree.Line(",");
                renderTree.InterpolatedLine($"{parameter.Name:q}");
                renderTree.EndBlock();
                renderTree.Line(")");

                if (i < parameters.Length - 1) {
                    renderTree.Line(",");
                }
            }

            renderTree.EndBlock();
            renderTree.Line(")");
        });
    }

    public static RendererExpressionFragment? MakeTypeParameterListFromDefinition(
        ExpressionFragment classBuilder,
        ImmutableArray<ITypeParameterSymbol> typeParameters
    ) {
        if (typeParameters.Length == 0) {
            return null;
        }

        return RendererExpressionFragment.For(renderTree => {
            renderTree.InterpolatedLine($".{nameof(ISyntheticMethodBuilder<>.WithTypeParameters)}(");
            renderTree.StartBlock();

            for (var i = 0; i < typeParameters.Length; i++) {
                var typeParameter = typeParameters[i];
                renderTree.Interpolate($"{classBuilder.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.Context)}.{nameof(ICodeGenerationContext.TypeParameter)}(");
                renderTree.StartBlock();
                renderTree.InterpolatedLine($"{typeParameter.Name:q}");
                renderTree.EndBlock();
                renderTree.Text(")");

                if (i < typeParameters.Length - 1) {
                    renderTree.Line(",");
                }
            }

            renderTree.EndBlock();
            renderTree.Line(")");
        });
    }

    public static RendererExpressionFragment DeclareInterceptorMethodFromDefinition(
        ExpressionFragment classBuilder,
        IMethodSymbol method,
        ExpressionFragment locationIdentifierExpression,
        ISyntheticCodeBlockResolutionContext resolutionContext
    ) {
        if (method.DeclaringSyntaxReferences is not [var declarationRef]) {
            throw new InvalidOperationException($"The method {method.ContainingType.Name}.{method.Name} was not declared in this compilation.");
        }

        if (declarationRef.GetSyntax() is not MethodDeclarationSyntax methodDeclarationSyntax) {
            throw new NotSupportedException();
        }

        var declareParameterList = MakeParameterListFromDefinition(classBuilder, method.Parameters)!;
        var declareTypeParameterList = MakeTypeParameterListFromDefinition(classBuilder, method.TypeParameters);
        var returnType = MakeSyntheticTypeExpressionForType(method.ReturnType);

        var declareBody = method.IsAbstract
            ? throw new NotSupportedException("")
            : MakeCodeBlockFromMethodDeclaration(
                classBuilder,
                methodDeclarationSyntax,
                method.ReturnType,
                resolutionContext
            );

        return DeclareInterceptorMethod(
            classBuilder,
            returnType,
            method.Name,
            method.DeclaredAccessibility.ToAccessModifiers(),
            locationIdentifierExpression,
            declareTypeParameterList,
            declareParameterList,
            declareBody
        );
    }

    public static RendererExpressionFragment DeclareInterceptorMethod(
        ExpressionFragment classBuilder,
        InvocationExpressionFragment getReturnTypeExpression,
        string name,
        AccessModifierList accessibility,
        ExpressionFragment locationIdentifierExpression,
        RendererExpressionFragment? declareTypeParameterList,
        RendererExpressionFragment declareParameterList,
        RendererExpressionFragment declareBody
    ) {
        return RendererExpressionFragment.For(renderTree => {
            renderTree.Interpolate($"{typeof(SyntheticClassBuilderExtensions)}.{nameof(SyntheticClassBuilderExtensions.DeclareInterceptorMethodBuilder)}");
            renderTree.InterpolatedLine(
                $"({classBuilder}.{nameof(ISimpleSyntheticClassBuilder.ΦCompilerApi)}, {name:q}, {classBuilder})"
            );

            renderTree.StartBlock();
            renderTree.InterpolatedLine(
                $".{nameof(ISyntheticInterceptorMethodBuilder<>.WithAttributes)}("
                + $"{typeof(CodeGenerationContextExtensions)}.{nameof(CodeGenerationContextExtensions.InterceptLocationAttribute)}(this.{nameof(SyntheticDeclarationDefinition.CodeGenContext)}, {locationIdentifierExpression})"
                + $")");
            renderTree.InterpolatedLine($".{nameof(ISyntheticInterceptorMethodBuilder<>.WithAccessibility)}({accessibility.RenderAsExpression})");
            renderTree.Interpolate($".{nameof(ISyntheticInterceptorMethodBuilder<>.WithReturnType)}(");
            renderTree.Node(getReturnTypeExpression);
            renderTree.Line(")");

            if (declareTypeParameterList is not null) {
                renderTree.Node(declareTypeParameterList);
            }

            if (declareParameterList is not null) {
                renderTree.Node(declareParameterList);
            }

            renderTree.Node(declareBody);
        });
    }

    private static TypeInfoFragment GetTypeKnownAtCompileTime(
        ITypeSymbol type
    ) {
        if (type is ITypeParameterSymbol) {
            return new TypeInfoFragment(typeof(CompileTimeUnknown));
        }

        return type.ToRenderFragment();
    }

    public static InvocationExpressionFragment MakeSyntheticTypeExpressionForType(
        ITypeSymbol type
    ) {
        if (type is not ITypeParameterSymbol { DeclaringMethod: null } lateBoundType) {
            return MakeCodeGenContextInvocation(nameof(SyntheticDeclarationDefinition.CodeGenContext.Type), $"typeof({type.ToRenderFragment().QualifiedTypeName})");
        }

        return new InvocationExpressionFragment(
            "this",
            nameof(SyntheticDeclarationDefinition.InternalResolveLateBoundType),
            [$"nameof({lateBoundType.Name})"]
        );
    }

    private static InvocationExpressionFragment MakeCodeGenContextInvocation(
        string methodName,
        params ImmutableArray<ExpressionFragment> arguments
    ) {
        return new InvocationExpressionFragment(
            "this",
            $"{nameof(SyntheticDeclarationDefinition.CodeGenContext)}.{methodName}",
            arguments
        );
    }
}