using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Common.Internal;
using Mumei.Roslyn.Common.Polyfill;
using Eq = Microsoft.CodeAnalysis.SymbolEqualityComparer;

namespace Mumei.CodeGen.DeclarationGenerator;

public sealed partial class DeclarationDefinitionGenerator {
    private static void GenerateCode(ICodeGenerationContext ctx, (ClassDeclarationSyntax DefinitionDeclaration, INamedTypeSymbol DefinitionType) inputs) {
        var syntheticClassDefinitionType = ctx.TypeFromCompilation(typeof(SyntheticClassDefinition<>));
        var syntheticMethodDefinitionType = ctx.TypeFromCompilation(typeof(SyntheticMethodDefinition));
        var syntheticInterceptorMethodDefinitionType = ctx.TypeFromCompilation(typeof(SyntheticInterceptorMethodDefinition));

        var (definitionDeclaration, definitionType) = inputs;
        var ns = ctx.Namespace(definitionType.ContainingNamespace);

        if (!definitionDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)) {
            throw new NotSupportedException();
        }

        var definitionCodeGenClass = ns.DeclareClass(definitionType.Name)
            .WithTypeParametersFrom(definitionType)
            .WithAccessibility(definitionType.DeclaredAccessibility.ToAccessModifiers() + AccessModifier.Partial);

        if (Eq.Default.Equals(definitionType.BaseType, syntheticClassDefinitionType.Construct(definitionType))) {
            EmitBindCompilerOutputMembersMethodForClass(ctx, definitionCodeGenClass, definitionType);
        } else if (Eq.Default.Equals(definitionType.BaseType, syntheticMethodDefinitionType)) {
            EmitInternalBindCompilerMethodForMethod(ctx, definitionCodeGenClass, definitionType);
        } else if (Eq.Default.Equals(definitionType.BaseType, syntheticInterceptorMethodDefinitionType)) { }

        ctx.EmitUnique(definitionType.Name, definitionCodeGenClass);
    }

    public static void EmitBindCompilerOutputMembersMethodForClass(
        ICodeGenerationContext ctx,
        ISimpleSyntheticClassBuilder definitionCodeGenClass,
        INamedTypeSymbol definitionType
    ) {
        var bindingMethod = definitionCodeGenClass.DeclareMethod<Action<ISyntheticClassBuilder<CompileTimeUnknown>>>(
                nameof(SyntheticClassDefinition<>.InternalBindCompilerOutputMembers)
            )
            .WithAccessibility(AccessModifier.Public + AccessModifier.Override)
            .WithReturnType(typeof(void))
            .WithParameters(
                ctx.Parameter(
                    ctx.TypeFromCompilation(typeof(ISyntheticClassBuilder<>)).Construct(definitionType),
                    $"{Strings.PrivateLocal}builder",
                    out var builderParameter
                )
            );

        var resolveClassDynamicallyBoundType = $"{builderParameter.Value}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticClassBuilder<>.ΦCompilerApi.DynamicallyBoundType)}";
        var bindingExpressions = CollectOutputMemberBindingExpressions(ctx, definitionType, builderParameter, resolveClassDynamicallyBoundType);
        bindingMethod.WithBody(ctx.Block(bindingExpressions, static (renderTree, bindingExpressions) => {
            for (var i = 0; i < bindingExpressions.Length; i++) {
                var bindingExpression = bindingExpressions[i];
                renderTree.Node(bindingExpression);
                if (i < bindingExpressions.Length - 1) {
                    renderTree.NewLine();
                }
            }
        }));
    }


    public static void EmitInternalBindCompilerMethodForMethod(
        ICodeGenerationContext ctx,
        ISimpleSyntheticClassBuilder definitionCodeGenClass,
        INamedTypeSymbol definitionType
    ) {
        var bindingMethod = definitionCodeGenClass.DeclareMethod<Action<ISyntheticClassBuilder<CompileTimeUnknown>>>(
                nameof(SyntheticMethodDefinition.InternalBindCompilerMethod)
            )
            .WithAccessibility(AccessModifier.Public + AccessModifier.Override)
            .WithReturnType(typeof(ISyntheticMethodBuilder<Delegate>))
            .WithParameters(
                ctx.Parameter(
                    ctx.TypeFromCompilation(typeof(ISimpleSyntheticClassBuilder)),
                    $"{Strings.PrivateLocal}builder",
                    out var builderParameter
                ),
                ctx.Parameter(
                    ctx.TypeFromCompilation(typeof(MethodDefinitionBindingContext)),
                    $"{Strings.PrivateLocal}bindingContext",
                    out var bindingContextParameter
                ),
                ctx.Parameter(
                    ctx.TypeFromCompilation(typeof(Delegate)),
                    $"{Strings.PrivateLocal}targetMethod",
                    out var targetMethodParameter
                )
            );

        var outputMembers = new ArrayBuilder<ISymbol>();
        var inputMembers = new ArrayBuilder<ISymbol>();

        CollectOutputAndInputMembers(ctx, definitionType, ref outputMembers, ref inputMembers);

        var inputMemberNames = new HashSet<string>();
        foreach (var inputMember in inputMembers) {
            inputMemberNames.Add(inputMember.Name);
        }

        var createdOutputMethods = new ArrayBuilder<(IMethodSymbol Source, ISyntheticMethod BinderImpl)>();
        Span<char> uniqueNameBuffer = stackalloc char[ArrayBuilder.InitSize];
        foreach (var outputMember in outputMembers) {
            if (outputMember is not IMethodSymbol method) {
                continue;
            }

            var uniqueNameBuilder = new ArrayBuilder<char>(uniqueNameBuffer);
            uniqueNameBuilder.AddRange(Strings.PrivateIdentifier);
            uniqueNameBuilder.AddRange("BindMethod__");
            uniqueNameBuilder.AddRange(method.Name);
            uniqueNameBuilder.AddRange("_T");
            uniqueNameBuilder.EnsureCapacity(char.MaxAsciiIntLength);
            uniqueNameBuilder.Advance(char.WriteIntAsAsciiChars((uint) method.Arity, uniqueNameBuilder.UnsafeBuffer[uniqueNameBuilder.Count..]));

            foreach (var param in method.Parameters) {
                uniqueNameBuilder.Add('_');
                uniqueNameBuilder.AddRange(param.Name);
            }

            var renderExpression = MakeMethodOutputMemberBindingExpression(
                method,
                builderParameter,
                $"{bindingContextParameter.Value}.{nameof(MethodDefinitionBindingContext.ResolveDynamicallyBoundType)}",
                ctx.Compilation.GetSemanticModel(method.DeclaringSyntaxReferences[0].GetSyntax().SyntaxTree),
                inputMemberNames
            );

            var impl = definitionCodeGenClass.DeclareMethod<Delegate>(uniqueNameBuilder.ToStringAndFree())
                .WithAccessibility(AccessModifier.Private)
                .WithReturnType(typeof(ISyntheticMethodBuilder<Delegate>))
                .WithParameters(
                    ctx.Parameter(
                        ctx.Type(typeof(ISimpleSyntheticClassBuilder)),
                        builderParameter.Value
                    ),
                    ctx.Parameter(
                        ctx.Type(typeof(MethodDefinitionBindingContext)),
                        bindingContextParameter.Value
                    )
                ).WithBody(ctx.Block(renderExpression, static (renderTree, bindingExpressions) => {
                    renderTree.Text("return ");
                    renderTree.Node(bindingExpressions);
                }));

            createdOutputMethods.Add((method, impl));
        }

        bindingMethod.WithBody(ctx.Block((targetMethodParameter, builderParameter, bindingContextParameter, createdOutputMethods.ToArrayAndFree()), static (renderTree, inputs) => {
            var (targetMethodParameter, builderParameter, bindingContextParameter, outputMembers) = inputs;
            foreach (var outputMember in outputMembers) {
                renderTree.InterpolatedLine($$"""if ({{targetMethodParameter}}.Method == ((Delegate) {{outputMember.Source.Name}}).Method) {""");
                renderTree.StartBlock();
                renderTree.InterpolatedLine($$"""return {{outputMember.BinderImpl.Name.ConstantValue}}({{builderParameter}}, {{bindingContextParameter}});""");
                renderTree.EndBlock();
                renderTree.Line("}");
            }

            renderTree.InterpolatedLine($$"""throw new InvalidOperationException("Unsupported method");""");
        }));
    }

    public static void EmitInternalBindCompilerMethodForInterceptorMethod(
        ICodeGenerationContext ctx,
        ISimpleSyntheticClassBuilder definitionCodeGenClass,
        INamedTypeSymbol definitionType
    ) { }
}