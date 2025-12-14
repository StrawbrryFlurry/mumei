using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Common.Internal;
using Mumei.Roslyn.Common.Polyfill;

namespace Mumei.CodeGen.DeclarationGenerator;

public sealed partial class DeclarationDefinitionGenerator {

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
                $"this.{nameof(SyntheticDeclarationDefinition.InternalResolveLateBoundType)}",
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
                    )
                ).WithBody(ctx.Block(renderExpression, static (renderTree, bindingExpressions) => {
                    renderTree.Text("return ");
                    renderTree.Node(bindingExpressions);
                }));

            createdOutputMethods.Add((method, impl));
        }

        bindingMethod.WithBody(ctx.Block((targetMethodParameter, builderParameter, createdOutputMethods.ToArrayAndFree()), static (renderTree, inputs) => {
            var (targetMethodParameter, builderParameter, outputMembers) = inputs;
            foreach (var outputMember in outputMembers) {
                renderTree.InterpolatedLine($$"""if ({{targetMethodParameter}}.Method == ((Delegate) {{outputMember.Source.Name}}).Method) {""");
                renderTree.StartBlock();
                renderTree.InterpolatedLine($$"""return {{outputMember.BinderImpl.Name.ConstantValue}}({{builderParameter}});""");
                renderTree.EndBlock();
                renderTree.Line("}");
            }

            renderTree.InterpolatedLine($$"""throw new InvalidOperationException("Unsupported method");""");
        }));
    }

}