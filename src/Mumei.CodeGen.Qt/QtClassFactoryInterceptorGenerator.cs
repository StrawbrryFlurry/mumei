﻿using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Playground.Qt;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Roslyn;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Mumei.CodeGen.Qt;

[Generator]
public sealed class QtClassFactoryInterceptorGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var dynamicSourceCodeProvider = context.SyntaxProvider.CreateSyntaxProvider(
            (node, _) => node is InvocationExpressionSyntax {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "BindDynamicTemplateInterceptMethod" }
            } invocation,
            (ctx, _) => {
                return (ctx.Node, ctx.SemanticModel);
            }
        );

        context.RegisterSourceOutput(
            dynamicSourceCodeProvider.Collect(),
            (ctx, o) => Execute(ctx, o)
        );
    }

    private void Execute(SourceProductionContext context, ImmutableArray<(SyntaxNode Node, SemanticModel SemanticModel)> code) {
        var result = new SyntaxWriter();
        result.WriteLine("// <auto-generated/>");
        result.WriteLine("#nullable enable");
        result.WriteLine(
            """
            #pragma warning disable
            namespace System.Runtime.CompilerServices {
                [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                file sealed class InterceptsLocationAttribute(int version, string data) : Attribute;
            }
            #pragma warning enable

            namespace Mumei.CodeGen.Qt.Generated {
            """
        );

        result.Indent();

        result.WriteLine("file static class __QtInterceptorImpl {");
        result.Indent();

        foreach (var (node, semanticModel) in code) {
            WriteInterceptorMethod(
                result,
                node as InvocationExpressionSyntax ?? throw new InvalidOperationException("Expected InvocationExpressionSyntax"),
                semanticModel
            );
        }

        result.Dedent();
        result.WriteLine("}");
        result.Dedent();
        result.WriteLine("}");

        context.AddSource("QtClassFactoryInterceptor.g.cs", result.ToSyntax());
    }

    private void WriteInterceptorMethod(
        SyntaxWriter writer,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel
    ) {
        var proxyId = NextId;
        var methodDeclaration = invocation.ArgumentList.Arguments[1];

        var method = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation).Symbol!;
        var v = new QtInterceptorMethodDeclarationVisitor(semanticModel).Visit(methodDeclaration.Expression);
        BlockSyntax body;
        if (v is ParenthesizedLambdaExpressionSyntax complex) {
            body = (BlockSyntax)complex.Body;
        }
        else {
            body = (BlockSyntax)((SimpleLambdaExpressionSyntax)v).Body;
        }

        var bodyStatements = body.Statements.ToFullString();
        var sourceCodeSections = MakeDynamicallyBoundSourceCodeSections(bodyStatements);

        writer.WriteFormattedLine($"private static readonly string[] CachedSourceCodeTemplate_Intercept_{proxyId} = [");
        var ind = writer.IndentLevel;
        writer.SetIndentLevel(0);
        for (var i = 0; i < sourceCodeSections.Length; i++) {
            if (i > 0) {
                writer.WriteLine(",");
            }

            writer.WriteLine("\"\"\"\"\"\"\"\"\"\"\"\"");
            var sourceCodeSection = sourceCodeSections[i];
            writer.WriteLine(sourceCodeSection);
            writer.Write("\"\"\"\"\"\"\"\"\"\"\"\"");
        }

        writer.SetIndentLevel(ind);
        writer.WriteLine();
        writer.WriteLine("];");
        writer.WriteLine();

        var location = semanticModel.GetInterceptableLocation(invocation);
        writer.WriteLine(location!.GetInterceptsLocationAttributeSyntax());
        writer.WriteFormattedLine($"public static {typeof(QtMethod<CompileTimeUnknown>):g} Intercept__{proxyId}(");
        writer.Indent();
        writer.WriteFormattedLine($"in this {typeof(QtClass):g} self,");
        writer.WriteFormattedLine($"{typeof(InvocationExpressionSyntax):g} invocationToProxy,");

        var templateMethodType = method.TypeParameters.IsEmpty
            ? QtType.ForRuntimeType<DeclareQtInterceptorVoidMethod>()
            : QtType.ConstructRuntimeGenericType(typeof(DeclareQtInterceptorMethod<>), QtType.ForRoslynType(method.TypeArguments[0]));

        writer.WriteFormattedLine(
            $"{templateMethodType:g} declaration"
        );

        writer.Dedent();
        writer.WriteLine(") {");
        writer.Indent();

        writer.WriteFormattedBlock(
            $$"""
              var sourceCode = new {{typeof(__DynamicallyBoundSourceCode):g}}() {
                  {{nameof(__DynamicallyBoundSourceCode.CodeTemplate)}} = CachedSourceCodeTemplate_Intercept_{{proxyId}},
              };

              return self.__BindDynamicTemplateInterceptMethod(
                invocationToProxy,
                sourceCode
              );
              """
        );

        writer.Dedent();
        writer.WriteLine("}");
        writer.WriteLine();
    }

    private static string[] MakeDynamicallyBoundSourceCodeSections(
        ReadOnlySpan<char> code
    ) {
        var source = CleanUpCapturedSourceCode(code).AsSpan();
        // We could prolly make the binding of syntax nodes / context objs to the source code fully compile-time as well
        // Since we know all arguments and references of "external" code at this point.
        var result = ImmutableArray.CreateBuilder<string>();
        while (true) {
            var markerIdx = source.IndexOf(__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeStart);
            if (markerIdx == -1) {
                result.Add(source.ToString());
                break;
            }

            result.Add(source[..markerIdx].ToString());
            source = source[markerIdx..];
            var endIdx = source.IndexOf(__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeEnd);
            Debug.Assert(endIdx != -1);
            var identifier = source[..endIdx].ToString();
            result.Add(identifier);
            source = source[(endIdx + 1)..];
        }

        return result.ToArray();
    }

    private static string CleanUpCapturedSourceCode(
        ReadOnlySpan<char> source
    ) {
        var whitespaceToSkip = DetermineAccidentalTriviaIndentCount(source);
        var result = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);

        while (source.Length != 0) {
            var lineEnd = source.IndexOf("\n");
            lineEnd = lineEnd == -1 ? source.Length : lineEnd + 1; // Include the newline character in the line

            var line = source[..lineEnd];

            var whitespaceCount = 0;
            var maxWhiteSpaceToSkip = Math.Min(whitespaceToSkip, line.Length);
            for (var j = 0; j < maxWhiteSpaceToSkip; j++) {
                if (!char.IsWhiteSpace(line[j]) || line[j] == '\n' || line[j] == '\r') {
                    break;
                }

                whitespaceCount++;
            }

            var charsToRemove = Math.Min(whitespaceCount, whitespaceToSkip);

            result.Write(line[charsToRemove..]);
            source = source[lineEnd..];
        }

        return result.ToString();
    }

    private static int DetermineAccidentalTriviaIndentCount(in ReadOnlySpan<char> source) {
        var indentChars = 0;
        while (source[indentChars] is ' ' or '\t' && source[indentChars] != '\n' && indentChars < source.Length) {
            indentChars++;
        }

        return indentChars;
    }

    private sealed class QtInterceptorMethodDeclarationVisitor(
        SemanticModel sm
    ) : TypeToGloballyQualifiedIdentifierRewriter(sm) {
        private string? _ctxIdentifier;

        public override SyntaxNode? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) {
            if (_ctxIdentifier is not null) {
                return base.VisitSimpleLambdaExpression(node);
            }

            _ctxIdentifier = node.Parameter.Identifier.Text;
            return base.VisitSimpleLambdaExpression(node);
        }

        public override SyntaxNode? VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) {
            if (_ctxIdentifier is not null) {
                base.VisitParenthesizedLambdaExpression(node);
            }

            if (node.ParameterList.Parameters.Count == 1) {
                _ctxIdentifier = node.ParameterList.Parameters[0].Identifier.Text;
            }
            else {
                throw new InvalidOperationException("Expected a single parameter in the lambda expression.");
            }

            return base.VisitParenthesizedLambdaExpression(node);
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node) {
            Debug.Assert(_ctxIdentifier is not null);
            if (node.Expression is not MemberAccessExpressionSyntax memberAccess) {
                return base.VisitInvocationExpression(node);
            }

            if (memberAccess.Expression is not IdentifierNameSyntax invokee) {
                return base.VisitInvocationExpression(node);
            }

            if (invokee.Identifier.Text != _ctxIdentifier) {
                return base.VisitInvocationExpression(node);
            }

            return MakeMakerLiteralFor(memberAccess.Name.Identifier, node);
        }

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
            if (node.Expression is not IdentifierNameSyntax identifier ||
                identifier.Identifier.Text != _ctxIdentifier) {
                return base.VisitMemberAccessExpression(node);
            }

            return MakeMakerLiteralFor(node.Name.Identifier, node);
        }

        private IdentifierNameSyntax MakeMakerLiteralFor(SyntaxToken identifier, SyntaxNode sourceNode) {
            var binderKey = identifier.Text switch {
                nameof(QtDynamicInterceptorMethodCtx.Invoke) => ProxyInvocationExpressionBindingContext.BindInvocation,
                nameof(QtDynamicInterceptorMethodCtx.Method) => ProxyInvocationExpressionBindingContext.BindMethodInfo,
                nameof(QtDynamicInterceptorMethodCtx.InvocationArguments) => ProxyInvocationExpressionBindingContext.BindArguments,
                _ => throw new ArgumentOutOfRangeException()
            };

            return IdentifierName(
                    $"{__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeStart}{binderKey}{__DynamicallyBoundSourceCode.DynamicallyBoundSourceCodeEnd}"
                ).WithLeadingTrivia(sourceNode.GetLeadingTrivia())
                .WithTrailingTrivia(sourceNode.GetTrailingTrivia());
        }
    }

    private static string NextId => Guid.NewGuid().ToString("N");

}