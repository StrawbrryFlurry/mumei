using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.Common.Internal;
using Mumei.Roslyn.Common.Polyfill;

namespace Mumei.CodeGen.DeclarationGenerator;

public sealed partial class DeclarationDefinitionGenerator {
    private const string ClassBuilderParameter = $"{Strings.PrivateLocal}builder";
    private const string LocationToInterceptParameter = $"{Strings.PrivateLocal}locationToIntercept";
    private const string TargetMethodParameter = $"{Strings.PrivateLocal}targetMethod";

    public static void EmitInternalBindCompilerInterceptorMethodForMethod(
        ICodeGenerationContext ctx,
        ISimpleSyntheticClassBuilder definitionCodeGenClass,
        ClassDeclarationSyntax definitionDeclaration,
        INamedTypeSymbol definitionType
    ) {
        var bindingMethod = definitionCodeGenClass.DeclareMethod<Action<ISyntheticClassBuilder<CompileTimeUnknown>>>(
                nameof(SyntheticMethodDefinition.InternalBindCompilerMethod)
            )
            .WithAccessibility(AccessModifier.Public + AccessModifier.Override)
            .WithReturnType(typeof(ISyntheticInterceptorMethodBuilder<Delegate>))
            .WithParameters(
                ctx.Parameter(
                    ctx.Type(typeof(ISimpleSyntheticClassBuilder)),
                    ClassBuilderParameter
                ),
                ctx.Parameter(
                    ctx.Type(typeof(IInvocationOperation)),
                    $"{Strings.PrivateLocal}invocationToBind"
                ),
                ctx.Parameter(
                    ctx.Type(typeof(InterceptableLocation)),
                    LocationToInterceptParameter
                ),
                ctx.Parameter(
                    ctx.Type(typeof(Delegate)),
                    TargetMethodParameter
                )
            );

        var outputMembers = new ArrayBuilder<ISymbol>();
        var inputMembers = new HashSet<string>();

        CollectOutputAndInputMembers(ctx, definitionType, inputMembers, ref outputMembers);

        var resolutionContext = new SyntheticInterceptorMethodDefinitionMethodCodeBlockResolutionContext(
            ctx.Compilation.GetSemanticModel(definitionDeclaration.SyntaxTree),
            inputMembers
        );

        var createdOutputMethods = new ArrayBuilder<(IMethodSymbol Source, ISyntheticMethod BinderImpl)>();
        Span<char> uniqueNameBuffer = stackalloc char[ArrayBuilder.InitSize];
        foreach (var outputMember in outputMembers) {
            var declaredMethod = MakeOutputInterceptorMethodDeclarationExpression(
                ctx,
                definitionCodeGenClass,
                outputMember,
                uniqueNameBuffer,
                resolutionContext
            );

            if (declaredMethod is { } method) {
                createdOutputMethods.Add(method);
            }
        }

        bindingMethod.WithBody(ctx.Block(createdOutputMethods.ToArrayAndFree(), static (renderTree, inputs) => {
            foreach (var outputMember in inputs) {
                renderTree.InterpolatedLine(
                    $$"""if ({{TargetMethodParameter}}.Method == ((Delegate) {{outputMember.Source.Name}}).Method) {""");
                renderTree.StartBlock();
                renderTree.InterpolatedLine(
                    $$"""return {{outputMember.BinderImpl.Name.ConstantValue}}({{ClassBuilderParameter}}, {{LocationToInterceptParameter}});""");
                renderTree.EndBlock();
                renderTree.Line("}");
            }

            renderTree.InterpolatedLine($$"""throw new InvalidOperationException("Unsupported method");""");
        }));
    }

    private static (IMethodSymbol Source, ISyntheticMethod BinderImpl)?
        MakeOutputInterceptorMethodDeclarationExpression(
            ICodeGenerationContext ctx,
            ISimpleSyntheticClassBuilder definitionCodeGenClass,
            ISymbol outputMember,
            Span<char> uniqueNameBuffer,
            SyntheticInterceptorMethodDefinitionMethodCodeBlockResolutionContext resolutionContext
        ) {
        if (outputMember is not IMethodSymbol method) {
            return null;
        }

        var uniqueNameBuilder = new ArrayBuilder<char>(uniqueNameBuffer);
        uniqueNameBuilder.AddRange(Strings.PrivateIdentifier);
        uniqueNameBuilder.AddRange("BindMethod__");
        uniqueNameBuilder.AddRange(method.Name);
        uniqueNameBuilder.AddRange("_T");
        uniqueNameBuilder.EnsureCapacity(char.MaxAsciiIntLength);
        uniqueNameBuilder.Advance(char.WriteIntAsAsciiChars((uint)method.Arity,
            uniqueNameBuilder.UnsafeBuffer[uniqueNameBuilder.Count..]));

        foreach (var param in method.Parameters) {
            uniqueNameBuilder.Add('_');
            uniqueNameBuilder.AddRange(param.Name);
        }

        var methodName = uniqueNameBuilder.ToStringAndFree();

        var interceptorMethodParameterNameField =
            definitionCodeGenClass.DeclareField<string[]>($"{methodName}__Parameters",
                new SyntheticRendererExpression(new RenderFragment<object>()));

        var declareMethodOnBuilderExpression = DeclarationBuilderFactory.DeclareInterceptorMethodFromDefinition(
            ClassBuilderParameter,
            method,
            LocationToInterceptParameter,
            resolutionContext
        );

        var impl = definitionCodeGenClass.DeclareMethod<Delegate>(methodName)
            .WithAccessibility(AccessModifier.Private)
            .WithReturnType(typeof(ISyntheticInterceptorMethodBuilder<Delegate>))
            .WithParameters(
                ctx.Parameter(
                    ctx.Type(typeof(ISimpleSyntheticClassBuilder)),
                    ClassBuilderParameter
                ),
                ctx.Parameter(
                    ctx.Type(typeof(InterceptableLocation)),
                    LocationToInterceptParameter
                )
            ).WithBody(ctx.Block(declareMethodOnBuilderExpression, static (renderTree, bindingExpressions) => {
                renderTree.Text("return ");
                renderTree.Node(bindingExpressions);
            }));

        return (method, impl);
    }
}