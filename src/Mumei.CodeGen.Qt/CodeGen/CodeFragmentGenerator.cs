using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Qt;
using Mumei.Roslyn.Common;

namespace Mumei.CodeGen.Qt;

[Generator]
internal sealed class CodeFragmentGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var codeFragmentInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            IsCodeFragmentInvocation,
            TranslateCodeFragmentInvocationProvider
        ).Where(x => x is not null).Select((x, _) => x!.Value);

        var interceptorMethods = codeFragmentInvocations.Select(CreateCodeFragmentInterceptorMethod);

        context.RegisterSourceOutput(interceptorMethods, (ctx, state) => {
            var (method, scope) = state;
            QtCompilationScope.SetActiveScope(scope.Compilation);
            var cls = QtClass.CreateObfuscated(AccessModifier.FileSealed, $"__CodeFragments_{method.Method.Name}");
            cls.AddMethod(ref method);
            var ns = QtNamespace.FromGeneratorAssemblyName("Generated", cls);
            var file = QtSourceFile.CreateObfuscated("CodeFragments.g").WithNamespace(ns);
            var fileTree = new SyntaxRenderTreeBuilder();
            file.Render(fileTree);
            ctx.AddSource(file.Name, fileTree.GetSourceText());
        });
    }

    private static bool IsCodeFragmentInvocation(SyntaxNode node, CancellationToken _) {
        if (node is not InvocationExpressionSyntax invocation) {
            return false;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.Text: nameof(CodeFragment.Create), Expression: IdentifierNameSyntax { Identifier.Text: nameof(CodeFragment) } }) {
            return false;
        }

        return true;
    }

    private static CodeFragmentInvocation? TranslateCodeFragmentInvocationProvider(GeneratorSyntaxContext context, CancellationToken _) {
        var sm = context.SemanticModel;
        var invocation = (InvocationExpressionSyntax) context.Node;

        if (sm.GetOperation(invocation) is not IInvocationOperation operation) {
            return null;
        }

        if (operation.TargetMethod.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != $"global::{typeof(CodeFragment).FullName}") {
            return null;
        }

        if (operation.Arguments is not [{ Value: IDelegateCreationOperation { Syntax: LambdaExpressionSyntax fragmentDeclaration } }]) {
            return null;
        }

        if (!CodeFragmentInvocation.TryCreate(operation, invocation, fragmentDeclaration, out var codeFragmentInvocation)) {
            return null;
        }

        return codeFragmentInvocation;
    }

    private static (QtMethod<CodeFragment>, QtCompilationScope) CreateCodeFragmentInterceptorMethod(CodeFragmentInvocation codeFragment, CancellationToken _) {
        var name = RandomNameGenerator.GenerateName($"__Intercept_CodeFragment_{Math.Abs(codeFragment.Location.GetHashCode())}");
        var parameters = QtParameterList.Builder(1);
        parameters[0] = new QtParameter {
            Name = "declareFragment",
            Type = QtType.ForRuntimeType<Action>(),
            DefaultValue = null,
            Attributes = ParameterAttributes.None
        };

        var declaredBody = new GloballyQualifyingSyntaxRewriter(codeFragment.SemanticModel).Visit(codeFragment.Expression.Body);
        var bodyContent = declaredBody.NormalizeWhitespace() is BlockSyntax blockSyntax
            ? blockSyntax.Statements.ToFullString()
            : declaredBody.ToFullString();

        var bodyCode = new SyntaxRenderTreeBuilder();
        var fragmentType = QtType.ForRuntimeType<CodeFragment>();
        var fragment = LiteralNode.ForRawString(bodyContent.TrimEnd('\r', '\n'));
        bodyCode.Interpolate($"return {fragmentType.FullName}.{nameof(CodeFragment.λCreate)}({fragment});");
        bodyCode.NewLine();
        var body = CodeBlockNode.ForCode(bodyCode.GetSourceText());

        var method = new QtMethod<CodeFragment>(
            name,
            AccessModifier.InternalStatic,
            QtType.ForRuntimeType<CodeFragment>(),
            new QtTypeParameterList(),
            parameters,
            body,
            QtAttributeList.Empty,
            new QtDeclarationPtr<QtMethodRenderNode>()
        );

        return (method, new QtCompilationScope { Compilation = codeFragment.SemanticModel.Compilation });
    }

    internal readonly struct CodeFragmentInvocation : IEquatable<CodeFragmentInvocation> {
        public InterceptableLocation Location { get; private init; }
        public LambdaExpressionSyntax Expression { get; private init; }
        public IInvocationOperation Operation { get; private init; }
        public SemanticModel SemanticModel { get; private init; }

        public object Dependencies { get; } // TDeps

        public static bool TryCreate(
            IInvocationOperation operation,
            InvocationExpressionSyntax invocation,
            LambdaExpressionSyntax lambda,
            out CodeFragmentInvocation? fragmentInvocation
        ) {
            var semanticModel = operation.SemanticModel;
            if (semanticModel is null) {
                fragmentInvocation = null;
                return false;
            }

            var location = semanticModel.GetInterceptableLocation(invocation);
            if (location is null) {
                fragmentInvocation = null;
                return false;
            }

            fragmentInvocation = new CodeFragmentInvocation {
                Operation = operation,
                Location = location,
                SemanticModel = semanticModel,
                Expression = lambda
            };

            return true;
        }

        public bool Equals(CodeFragmentInvocation other) {
            return Location.Equals(other.Location) && other.Expression.Body.GetText().ContentEquals(Expression.Body.GetText());
        }

        public override bool Equals(object? obj) {
            return obj is CodeFragmentInvocation other && Equals(other);
        }

        public override int GetHashCode() {
            return Location.GetHashCode();
        }
    }
}

/// <summary>
/// Represents a fragment of code that can be used in generated code.
/// <code>
/// CodeFragment.Create(() => {
///     // Code to be included in the generated output
/// });
/// </code>
/// </summary>
public sealed class CodeFragment {
    private readonly string _code;

    public static CodeFragment Create(Action declareFragment) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static CodeFragment Create<TDeps>(TDeps deps, Action<TDeps> declareFragment) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static CodeFragment Create(Func<Task> declareAsyncFragment) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static CodeFragment Create<TDeps>(TDeps deps, Func<TDeps, Task> declareAsyncFragment) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static CodeFragment λCreate(string code) {
        return new CodeFragment(code);
    }

    private CodeFragment(string code) {
        _code = code;
    }

    public override string ToString() {
        return _code;
    }
}