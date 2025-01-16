using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace Mumei.Roslyn.SourceCodeReferenceGenerator;

[Generator]
public class SourceCodeReferenceGenerator : IIncrementalGenerator {
    private const string SourceCodeFactoryMetadataName = "SourceCodeFactory.SourceCode";

    private const string SourceFactoryCode =
        """
        // <auto-generated/>

        namespace SourceCodeFactory; 

        internal interface ITypeRef {}

        internal sealed class SourceCodeTypeRef : ITypeRef {
          public string TypeName { get; init; }
          public string SourceCode { get; init; }
          public global::System.Collections.Immutable.ImmutableArray<ITypeRef> References { get; init; }
        }

        internal sealed class AssemblyTypeRef : ITypeRef {
          public string AssemblyName { get; init; }
          public string FullyQualifiedName { get; init; }
        }

        internal static class SourceCode {
          public static SourceCodeTypeRef Of<T>() {
              throw new global::System.NotImplementedException();
          }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "SourceCode.Factory.g.cs",
            SourceText.From(SourceFactoryCode, Encoding.UTF8))
        );

        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is InvocationExpressionSyntax {
                    Expression: MemberAccessExpressionSyntax {
                        Name.Identifier.ValueText: "Of", Expression: IdentifierNameSyntax { Identifier.Text: "SourceCode" }
                    }
                },
                (ctx, _) => {
                    var sourceCodeFactory = ctx.SemanticModel.Compilation.GetTypeByMetadataName(SourceCodeFactoryMetadataName);
                    var calledMethod = ctx.SemanticModel.GetSymbolInfo(ctx.Node).Symbol as IMethodSymbol;
                    var areEqual = SymbolEqualityComparer.Default.Equals(calledMethod?.ContainingType, sourceCodeFactory);
                    if (calledMethod is null || ctx.Node is not InvocationExpressionSyntax invocation || !areEqual) {
                        return (false, null!, null!);
                    }

                    var typeArgument = calledMethod.TypeArguments[0];
                    return ((bool IsMatch, InvocationExpressionSyntax Invocation, INamedTypeSymbol SourceType))(true, invocation, typeArgument);
                })
            .Where(t => t.IsMatch)
            .Select((t, _) => (t.Invocation, t.SourceType));

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, t.Left, t.Right)
        );
    }

    private static void GenerateCode(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<(InvocationExpressionSyntax Invocation, INamedTypeSymbol RefType)> refs
    ) {
        var interceptorClass = ClassDeclaration("SourceCodeFactory_Interceptor")
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword)));

        foreach (var r in refs) {
            AddInterceptorCallForMethod(ref interceptorClass, compilation, r.RefType, r.Invocation);
        }

        var interceptorAttribute = ParseCompilationUnit(
            """
            #pragma warning disable
            namespace System.Runtime.CompilerServices {
                [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                file sealed class InterceptsLocationAttribute(string filePath, int line, int column) : Attribute;
            }
            #pragma warning enable
            """).Members.First();

        var cu = CompilationUnit()
            .WithMembers(List((MemberDeclarationSyntax[]) [
                NamespaceDeclaration(ParseName(compilation.AssemblyName + ".Generated"))
                    .WithMembers(List<MemberDeclarationSyntax>([interceptorClass])),
                interceptorAttribute
            ]));

        context.AddSource("SourceCodeFactory.interceptor.g.cs", cu.NormalizeWhitespace().ToFullString());
    }

    private static void AddInterceptorCallForMethod(
        ref ClassDeclarationSyntax interceptorClass,
        Compilation compilation,
        INamedTypeSymbol targetType,
        InvocationExpressionSyntax invocation
    ) {
        var interceptMethod = MethodDeclaration(ParseTypeName("global::SourceCodeFactory.SourceCodeTypeRef"), $"Intercept_SourceCodeOf__{Guid.NewGuid():N}")
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithTypeParameterList(TypeParameterList(SeparatedList([TypeParameter("T")])));

        var body = Block(
            ReturnStatement(
                InstantiateSourceCodeTypeRef(compilation, targetType)
            )
        );

        var ofMethodLocation = invocation.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax
            ? memberAccessExpressionSyntax.Name.GetLocation().GetLineSpan()
            : throw new InvalidOperationException("Invalid invocation expression");

        var line = ofMethodLocation.StartLinePosition.Line + 1;
        var column = ofMethodLocation.StartLinePosition.Character + 1;

        interceptMethod = interceptMethod.WithBody(body)
            .WithAttributeLists(
                List([
                    AttributeList(SeparatedList([
                        Attribute(
                            ParseName("System.Runtime.CompilerServices.InterceptsLocation"),
                            AttributeArgumentList(SeparatedList([
                                AttributeArgument(
                                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(invocation.SyntaxTree.FilePath))
                                ),
                                AttributeArgument(
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(line))
                                ),
                                AttributeArgument(
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(column))
                                )
                            ]))
                        )
                    ]))
                ])
            );

        interceptorClass = interceptorClass.AddMembers(interceptMethod);
    }

    private static ObjectCreationExpressionSyntax InstantiateSourceCodeTypeRef(
        Compilation compilation,
        ITypeSymbol targetType,
        HashSet<ITypeSymbol>? processedTypes = null
    ) {
        processedTypes ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        var refNode = targetType.DeclaringSyntaxReferences.First().GetSyntax();
        var globalized = TypeToGloballyQualifiedIdentifierRewriter.GlobalizeIdentifiers(
            compilation.GetSemanticModel(refNode.SyntaxTree),
            refNode,
            out var typeReferences
        );
        var sourceCode = globalized.NormalizeWhitespace().ToFullString();
        var references = MakeSourceTypeReferencesArray(compilation, typeReferences, processedTypes);

        return ObjectCreationExpression(ParseTypeName("global::SourceCodeFactory.SourceCodeTypeRef"))
            .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                SeparatedList(new ExpressionSyntax[] {
                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("TypeName"),
                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(targetType.Name))
                    ),
                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("SourceCode"),
                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(sourceCode))
                    ),
                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("References"),
                        references
                    )
                })
            ));
    }

    private static ExpressionSyntax MakeSourceTypeReferencesArray(
        Compilation compilation,
        ImmutableArray<ITypeSymbol> typeReferences,
        HashSet<ITypeSymbol> processedTypes
    ) {
        var references = InvocationExpression(
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                ParseTypeName("global::System.Collections.Immutable.ImmutableArray"),
                GenericName("Create").WithTypeArgumentList(TypeArgumentList(SeparatedList([ParseTypeName("global::SourceCodeFactory.ITypeRef")])))
            )
        );

        if (typeReferences.Length == 0) {
            return references;
        }

        var elements = new List<ExpressionSyntax>();
        foreach (var type in typeReferences) {
            if (type.DeclaringSyntaxReferences.IsEmpty) {
                elements.Add(
                    ObjectCreationExpression(ParseTypeName("global::SourceCodeFactory.AssemblyTypeRef"))
                        .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                            SeparatedList(new ExpressionSyntax[] {
                                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("AssemblyName"),
                                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(type.ContainingAssembly.Name))
                                ),
                                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("FullyQualifiedName"),
                                    LiteralExpression(SyntaxKind.StringLiteralExpression,
                                        Literal(type.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.FullyQualifiedFormat))
                                    )
                                )
                            })
                        ))
                );

                continue;
            }

            elements.Add(InstantiateSourceCodeTypeRef(compilation, type, processedTypes));
        }

        return references.WithArgumentList(ArgumentList(SeparatedList(
            elements.Select(Argument)
        )));
    }
}