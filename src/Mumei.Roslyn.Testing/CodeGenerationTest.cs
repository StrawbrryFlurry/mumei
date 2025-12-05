using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Roslyn;

namespace Mumei.Roslyn.Testing;

public sealed class CodeGenerationTest(Compilation compilation, string? sourceNamespace) {
    public Compilation Compilation { get; init; } = compilation;
    public ICodeGenerationContext Context => field ??= CreateContext();

    public CodeGenerationTest IncrementalGenerate(Action<ICodeGenerationContext, CompilationFromSyntaxTree> generate) {
        var compilation = new CompilationFromSyntaxTree(Compilation, sourceNamespace);
        generate(Context, compilation);
        return this;
    }

    public CodeGenerationTest AssertFile(string trackingName, Action<SyntaxTree> assert) {
        var output = Context.ΦCompilerApi.EnumerateDeclarationsToEmit().SingleOrDefault(x => x.TrackingName.ConstantValue == trackingName);
        if (output.Declarations.IsDefaultOrEmpty) {
            throw new InvalidOperationException($"No generated file with tracking name '{trackingName}' found.");
        }

        var compilationUnit = ((CSharpCodeGenerationContext) Context).SynthesizeCompilationUnit(output.Declarations, null);

        var renderer = new SourceFileRenderTreeBuilder();
        var syntaxTreeContent = renderer.RenderRootNode(compilationUnit);

        var syntaxTree = CSharpSyntaxTree.ParseText(syntaxTreeContent);
        assert(syntaxTree);

        return this;
    }

    public static CodeGenerationTest ForSource(ICompilationReference reference) {
        return ForCompilation(b => b.AddReference(reference));
    }

    public static CodeGenerationTest ForCompilation(Action<TestCompilationBuilder> configureCompilation) {
        var builder = new TestCompilationBuilder();
        configureCompilation(builder);
        return new CodeGenerationTest(builder.Build(), builder.SourceNamespace);
    }

    private CSharpCodeGenerationContext CreateContext() {
        var ctx = new CSharpCodeGenerationContext();
        ctx.RegisterContextProvider(new CompilationCodeGenerationContextProvider(Compilation));
        return ctx;
    }
}