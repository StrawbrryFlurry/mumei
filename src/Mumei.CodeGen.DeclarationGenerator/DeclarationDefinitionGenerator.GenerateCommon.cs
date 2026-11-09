using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Legacy.Components;
using Mumei.CodeGen.Legacy.Rendering;
using Mumei.CodeGen.Legacy.Rendering.CSharp;
using Mumei.CodeGen.Legacy.Roslyn;
using Mumei.CodeGen.Legacy.Roslyn.Components;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.DeclarationGenerator;

public sealed partial class DeclarationDefinitionGenerator {
    private static void CollectOutputAndInputMembers(
        ICodeGenerationContext ctx,
        INamedTypeSymbol classDefinitionSymbol,
        ref ArrayBuilder<ISymbol> inputMembers,
        ref ArrayBuilder<ISymbol> outputMembers
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


    private static void CollectOutputAndInputMembers(
        ICodeGenerationContext ctx,
        INamedTypeSymbol classDefinitionSymbol,
        HashSet<string> inputMemberNames,
        ref ArrayBuilder<ISymbol> outputMembers
    ) {
        var outputAttribute = ctx.TypeFromCompilation<OutputAttribute>();
        var inputAttribute = ctx.TypeFromCompilation<InputAttribute>();

        foreach (var member in classDefinitionSymbol.GetMembers()) {
            if (member.HasAttribute(outputAttribute)) {
                outputMembers.Add(member);
            }

            if (member.HasAttribute(inputAttribute)) {
                inputMemberNames.Add(member.Name);
            }
        }
    }
}