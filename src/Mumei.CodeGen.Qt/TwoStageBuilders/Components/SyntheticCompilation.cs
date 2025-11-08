using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

/// <summary>
/// Stores synthetic code components that can be built into compilation units or Synthesized trees.
/// Components that reference other components use this to resolve those dependencies.
/// </summary>
public sealed class SyntheticCompilation(Compilation compilation) {
    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(string name) {
        return new SyntheticClassBuilder<CompileTimeUnknown>(this);
    }

    public ISyntheticMethod Method<TMethodDefinition>(
        Action<TMethodDefinition> inputBinder,
        Func<TMethodDefinition, Delegate> methodSelector
    ) where TMethodDefinition : SyntheticMethodDefinition, new() {
        return null!;
    }

    public ISyntheticMethod InterceptorMethod<TMethodDefinition>(
        InvocationExpressionSyntax invocationToIntercept,
        Action<TMethodDefinition> inputBinder,
        Func<TMethodDefinition, Delegate> methodSelector
    ) where TMethodDefinition : SyntheticMethodDefinition, new() {
        return null!;
    }

    public string MakeUniqueName(string name) {
        return null!;
    }

    public SynthesizedClassDeclaration Synthesize<TClass>(ISyntheticClassBuilder<TClass> builder) {
        return new SynthesizedClassDeclaration();
    }
}