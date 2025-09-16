using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Roslyn;
using Mumei.Roslyn.Common;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace Mumei.CodeGen.Qt;

public sealed partial class QtClassFactoryInterceptorGenerator {
    private void BindTemplateInterceptMethod(
        SyntaxWriter result,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        IInvocationOperation invocationOperation,
        SourceProductionContext context
    ) {
        var args = QtClassDynamicDeclarationExtensions.ParseAddTemplateInterceptMethodArguments(invocationOperation);
        var methodTemplateArg = invocation.ArgumentList.Arguments[1];
        if (args.Template.Value is not IObjectCreationOperation templateCreationOp) {
            Diagnostics.ReportMethodTemplateNotFromCallSite(context, methodTemplateArg);
            return;
        }

        if (templateCreationOp.Constructor is null) {
            throw new InvalidOperationException("Constructor of template factory reference is null.");
        }

        if (templateCreationOp.Constructor.DeclaringSyntaxReferences.Length == 0) {
            Diagnostics.ReportMethodTemplateNotDeclaredInCompilation(context, methodTemplateArg);
            return;
        }

        var constructorDeclaration = templateCreationOp.Constructor.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken);

        if (constructorDeclaration is not ClassDeclarationSyntax { ParameterList: { } primaryConstructor } templateClassDeclaration) {
            Diagnostics.ReportMethodTemplateUsesNonPrimaryConstructor(context, methodTemplateArg);
            return;
        }

        var templateType = templateCreationOp.Constructor.DeclaringType();
        var selectedMethod = GetMethodTemplateMethodFromSelection(args.MethodSelector, templateType, context);
        if (selectedMethod is null) {
            return;
        }

        var methodTemplateDeclaration = selectedMethod.DeclaringSyntaxReferences.Length == 0
            ? throw new NotSupportedException()
            : selectedMethod.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;


        var stateIdentifiers = primaryConstructor.Parameters.Select(x => x.Identifier.Text).ToImmutableHashSet();
        var methodParameters = selectedMethod.Parameters.Select(x => x.Name).ToImmutableHashSet();
        var visitor = new QtMethodTemplateDeclarationVisitor(semanticModel, templateType, stateIdentifiers, methodParameters);

        var updatedMethodDeclaration = visitor.Visit(methodTemplateDeclaration) as MethodDeclarationSyntax;
        var body = updatedMethodDeclaration!.Body;

        var sourceCodeSections = MakeDynamicallyBoundSourceCodeSections(body!.Statements, out var boundKeys);
        var proxyId = NextId;
        WriteCachedSourceCodeDeclaration(result, proxyId, sourceCodeSections);


        // The template type might be private or file scoped, so we can't use it directly
        var methodTemplateTypeArg = QtType.ForExpression(QtExpression.For("TMethodTemplate"));
        var methodSelectorType = QtType.ConstructRuntimeGenericType(typeof(Func<>), methodTemplateTypeArg, QtType.ForRuntimeType<Delegate>());

        var instantiateTemplateReferenceBindersExpr = CreateTemplateBindersExpressionForMethodTemplate(boundKeys);

        var location = CSharpExtensions.GetInterceptableLocation(semanticModel, invocation);
        result.WriteLine(CSharpExtensions.GetInterceptsLocationAttributeSyntax(location!));
        result.WriteFormattedBlock(
            $$"""
              public static {{typeof(QtMethod<CompileTimeUnknown>)}} Intercept_{{nameof(QtClassDynamicDeclarationExtensions.AddTemplateInterceptMethod)}}_{{proxyId}}<TMethodTemplate>(
                  ref this {{typeof(QtClass)}} self,
                  {{typeof(InvocationExpressionSyntax)}} invocationToProxy,
                  TMethodTemplate methodTemplateInstance,
                  {{methodSelectorType}} templateMethodSelector
              ) {
                  var sourceCode = new {{typeof(__DynamicallyBoundSourceCode)}}() {
                      {{nameof(__DynamicallyBoundSourceCode.CodeTemplate)}} = CachedSourceCodeTemplate_Intercept_{{proxyId}},
                  };

                  var dynamicComponentBinders = {{instantiateTemplateReferenceBindersExpr}};
                  return self.{{nameof(QtClass.__BindDynamicTemplateInterceptMethod)}}(
                    invocationToProxy,
                    sourceCode,
                    dynamicComponentBinders
                  );
              }
              """
        );
    }

    private QtExpression CreateTemplateBindersExpressionForMethodTemplate(string[] boundKeys) {
        if (boundKeys.Length == 0) {
            return QtExpression.Null;
        }

        return QtExpression.ForExpression($"new {typeof(QtDynamicComponentBinderCollection)}()");
    }

    private IMethodSymbol? GetMethodTemplateMethodFromSelection(
        IArgumentOperation methodSelectionArg,
        ITypeSymbol declaringType,
        SourceProductionContext context
    ) {
        if (methodSelectionArg.Syntax is not ArgumentSyntax { Expression: LambdaExpressionSyntax lambda } argument) {
            throw new InvalidOperationException("Expected method selection argument to be a lambda expression.");
        }

        if (lambda.Body is not MemberAccessExpressionSyntax memberAccess) {
            Diagnostics.ReportRequireSimpleMethodTemplateMethodSelector(context, argument);
            return null;
        }

        var selectedMethodSymbol = methodSelectionArg.SemanticModel!.GetSymbolInfo(memberAccess.Name);
        if (selectedMethodSymbol.CandidateSymbols.Length > 1 && selectedMethodSymbol.CandidateReason == CandidateReason.OverloadResolutionFailure) {
            var bestCandidate = selectedMethodSymbol.CandidateSymbols.FirstOrDefault(x => x is IMethodSymbol ms && SymbolEqualityComparer.Default.Equals(ms.DeclaringType(), declaringType));
            if (bestCandidate is not null) {
                return (IMethodSymbol) bestCandidate;
            }
        }

        if (selectedMethodSymbol.Symbol is not IMethodSymbol methodSymbol) {
            Diagnostics.ReportMethodTemplateMethodSelectorSelectedNonTemplateMethod(context, argument);
            return null;
        }

        return methodSymbol;
    }

}

/// <summary>
/// Responsible for ensuring that we can bind all "dynamic" components of the method body.
/// Operations:
/// - Member access to state (provided via the ctor) => Mark For State Binding
///     (this.stateMember, stateMember)
/// - Local access to method parameters => Mark For Parameter Binding
///     (localParam)
/// - Member access to inherited template members => Mark For Proxy Binding
///     (this.Invoke(), Method (implicit), this.Arguments)
/// - Member access to instance members => Mark For Member Binding
///     (this.instanceMember, instanceMember)
/// - Simple instance access => Mark For Member Binding
///     (this)
/// </summary>
/// <param name="sm"></param>
/// <param name="stateIdentifiers"></param>
/// <param name="parameterIdentifiers"></param>
internal sealed class QtMethodTemplateDeclarationVisitor(
    SemanticModel sm,
    ITypeSymbol templateType,
    ImmutableHashSet<string> stateIdentifiers,
    ImmutableHashSet<string> parameterIdentifiers
) : GloballyQualifyingSyntaxRewriter(sm) {
    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node) {
        // Since we need to replace the entire invocation we can't rely on the identifier visitor
        if (TryReplaceCompileTimeCastWithThis(node, out var castedThisExpr)) {
            return castedThisExpr;
        }

        // Unless the invocation needs to be bound to Invoke, we can simply delegate
        // replacing identifiers to the identifier visitor. Invoke needs to replace
        // the entire invocation, since we just need the identifier at this position
        SimpleNameSyntax methodName;
        if (node.Expression is SimpleNameSyntax identifier) {
            methodName = identifier;
            goto TryBindInvokeMethod;
        }

        if (node.Expression is MemberAccessExpressionSyntax memberAccess) {
            if (memberAccess.Expression is not ThisExpressionSyntax) {
                return base.VisitInvocationExpression(node);
            }

            methodName = memberAccess.Name;
            goto TryBindInvokeMethod;
        }

        return base.VisitInvocationExpression(node);

        TryBindInvokeMethod:
        var couldBindInvokeMethod = TryGetMethodTemplateBindingKey(methodName.Identifier.Text, out var invokeBindingKey) && invokeBindingKey.Kind == ProxyMethodBindingKeys.Invoke;
        if (!couldBindInvokeMethod) {
            return base.VisitInvocationExpression(node);
        }

        if (SemanticModel.GetSymbolInfo(node).Symbol is not IMethodSymbol { IsStatic: false } methodSymbol) {
            return base.VisitInvocationExpression(node);
        }

        // TODO: Add cache for types and do comparisons with the actual symbol
        if (methodSymbol.ContainingType.Name != nameof(QtInterceptorMethodTemplate)) {
            return base.VisitInvocationExpression(node);
        }

        return Bind(invokeBindingKey, node);
    }

    private bool TryReplaceCompileTimeCastWithThis(InvocationExpressionSyntax invocation, out SyntaxNode? castedThisExpr) {
        // TODO: Should we track what casts are performed here and validate
        SimpleNameSyntax methodName;

        // Implicit Is<T>() call
        if (invocation.Expression is SimpleNameSyntax methodNameSyntax) {
            methodName = methodNameSyntax;
            goto CheckName;
        }

        // Explicit this.Is<T>() call
        if (invocation.Expression is not MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccess) {
            castedThisExpr = null;
            return false;
        }

        castedThisExpr = memberAccess.Expression;
        methodName = memberAccess.Name;

        CheckName:
        if (methodName is not GenericNameSyntax { TypeArgumentList.Arguments.Count: 1, Identifier.Text: nameof(IQtThis.Is) }) {
            castedThisExpr = null;
            return false;
        }

        castedThisExpr = Bind(TemplateBindingKey.For(ProxyMethodBindingKeys.This), invocation);
        return true;
    }

    public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
        if (node.Expression is not ThisExpressionSyntax) {
            return base.VisitMemberAccessExpression(node);
        }

        // Simply strip away the "this"
        if (TryCreateBindingForName(node.Name, node, out var binding)) {
            return binding;
        }

        return base.VisitMemberAccessExpression(node);
    }

    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
        if (TryCreateBindingForName(node, node, out var binding)) {
            return binding;
        }

        return base.VisitIdentifierName(node);
    }

    public override SyntaxNode? VisitGenericName(GenericNameSyntax node) {
        if (TryCreateBindingForName(node, node, out var binding)) {
            return binding;
        }

        return base.VisitGenericName(node);
    }

    public override SyntaxNode? VisitThisExpression(ThisExpressionSyntax node) {
        var binding = Bind(TemplateBindingKey.For(ProxyMethodBindingKeys.This), node);
        return binding;
    }

    private bool TryCreateBindingForName(SimpleNameSyntax node, SyntaxNode containingNode, [NotNullWhen(true)] out IdentifierNameSyntax? binding) {
        var identifier = node.Identifier.Text;
        var isVariableIdentifier = node.Parent is VariableDeclaratorSyntax declarator && declarator.Identifier.Text == identifier;
        if (isVariableIdentifier) {
            binding = null;
            return false;
        }

        if (stateIdentifiers.Contains(identifier)) {
            binding = Bind(TemplateBindingKey.For(ProxyMethodBindingKeys.State, identifier), containingNode);
            return true;
        }

        if (parameterIdentifiers.Contains(identifier)) {
            binding = Bind(TemplateBindingKey.For(ProxyMethodBindingKeys.Parameter, identifier), containingNode);
            return true;
        }

        if (TryGetMethodTemplateBindingKey(identifier, out var templateBindingKey)) {
            binding = Bind(templateBindingKey, containingNode);
            return true;
        }

        // At this point the only "bindable" things left are instance members
        // Try to resolve what instance member we are dealing with, if any
        if (SemanticModel.GetSymbolInfo(node) is not { Symbol: { } symbol }) {
            binding = null;
            return false;
        }

        // TODO: Support generating static methods into the template class
        if (symbol is not IPropertySymbol { IsStatic: false } and not IFieldSymbol { IsStatic: false } and not IMethodSymbol { IsStatic: false }) {
            binding = null;
            return false;
        }

        // TODO: We should prolly differentiate between instance members of the template vs the instance members of the proxy class
        // Maybe declare them as part of the QtMethodTemplate base class?
        var isMemberAccessOfTemplate = SymbolEqualityComparer.Default.Equals(symbol.ContainingType, templateType);
        if (isMemberAccessOfTemplate) {
            binding = Bind(TemplateBindingKey.For(ProxyMethodBindingKeys.Member, symbol.Name), containingNode);
            return true;
        }

        binding = null;
        return false;
    }

    private bool TryGetMethodTemplateBindingKey(string identifier, out TemplateBindingKey bindingKey) {
        var x = identifier switch {
            nameof(QtInterceptorMethodTemplate.Invoke) => ProxyMethodBindingKeys.Invoke,
            nameof(QtInterceptorMethodTemplate.Method) => ProxyMethodBindingKeys.MethodInfo,
            nameof(QtInterceptorMethodTemplate.InvocationArguments) => ProxyMethodBindingKeys.ArgumentList,
            _ => null
        };

        if (x is null) {
            bindingKey = default;
            return false;
        }

        bindingKey = TemplateBindingKey.For(x);
        return true;
    }

    private IdentifierNameSyntax Bind(TemplateBindingKey bindingKey, SyntaxNode sourceNode) {
        return IdentifierName(__DynamicallyBoundSourceCode.MakeDynamicSection(bindingKey))
            .WithLeadingTrivia(sourceNode.GetLeadingTrivia())
            .WithTrailingTrivia(sourceNode.GetTrailingTrivia());
    }
}