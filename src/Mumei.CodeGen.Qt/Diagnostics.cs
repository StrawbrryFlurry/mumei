using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Qt;

internal static class Diagnostics {
    private static readonly DiagnosticDescriptor MethodTemplateArgumentNotFromCallSite = new(
        "MOOM0001",
        "Method Templates need to be constructed at the registration site",
        "The Method Template argument needs to be constructed at the registration site and cannot be a reference to an outside instance",
        "Compile Time Classes",
        DiagnosticSeverity.Error,
        true,
        "Method Templates need to be constructed at the registration site."
    );

    public static void ReportMethodTemplateNotFromCallSite(SourceProductionContext ctx, ArgumentSyntax templateArgument) {
        var diagnostic = Diagnostic.Create(MethodTemplateArgumentNotFromCallSite, templateArgument.GetLocation());
        ctx.ReportDiagnostic(diagnostic);
    }

    private static readonly DiagnosticDescriptor MethodTemplateUsesNonPrimaryConstructor = new(
        "MOOM0002",
        "Method Templates need to be constructed using their primary constructor",
        "The Method Template provided needs to be constructed using its primary constructor",
        "Compile Time Classes",
        DiagnosticSeverity.Error,
        true,
        "Method Templates need to be constructed at the registration site."
    );

    public static void ReportMethodTemplateUsesNonPrimaryConstructor(SourceProductionContext context, ArgumentSyntax templateArgument) {
        var diagnostic = Diagnostic.Create(MethodTemplateUsesNonPrimaryConstructor, templateArgument.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static readonly DiagnosticDescriptor MethodTemplateNotDeclaredInCompilation = new(
        "MOOM0003",
        "Method Templates need to be declared in the current compilation",
        "The Method Template provided needs to be part of the current compilation",
        "Compile Time Classes",
        DiagnosticSeverity.Error,
        true,
        "Method Templates need to be declared in the current compilation."
    );

    public static void ReportMethodTemplateNotDeclaredInCompilation(SourceProductionContext context, ArgumentSyntax templateArgument) {
        var diagnostic = Diagnostic.Create(MethodTemplateUsesNonPrimaryConstructor, templateArgument.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static readonly DiagnosticDescriptor RequireSimpleMethodTemplateMethodSelector = new(
        "MOOM0004",
        "Method Template method selector should be simple",
        "The Method Template method selector should be a simple member access expression",
        "Compile Time Classes",
        DiagnosticSeverity.Error,
        true,
        "Method Template constructor arguments need to be constant values."
    );

    public static void ReportRequireSimpleMethodTemplateMethodSelector(SourceProductionContext context, ArgumentSyntax methodSelectorArgument) {
        var diagnostic = Diagnostic.Create(RequireSimpleMethodTemplateMethodSelector, methodSelectorArgument.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static readonly DiagnosticDescriptor MethodTemplateMethodSelectorSelectedNonTemplateMethod = new(
        "MOOM0005",
        "Method Template method selector should select a template method",
        "The selected method for the Method Template needs to be declared in the provided Method Template class",
        "Compile Time Classes",
        DiagnosticSeverity.Error,
        true,
        "Method Template constructor arguments need to be constant values."
    );

    public static void ReportMethodTemplateMethodSelectorSelectedNonTemplateMethod(SourceProductionContext context, ArgumentSyntax methodSelectorArgument) {
        var diagnostic = Diagnostic.Create(MethodTemplateMethodSelectorSelectedNonTemplateMethod, methodSelectorArgument.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

}