using System.Diagnostics.CodeAnalysis;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticMethodBuilder<TSignature> : ISyntheticMethod<TSignature> where TSignature : Delegate {
    [Experimental(Diagnostics.InternalFeatureId)]
    public λIInternalMethodBuilderCompilerApi λCompilerApi { get; }

    public ISyntheticMethodBuilder<TSignature> WithBody(TSignature bodyImpl);
    public ISyntheticMethodBuilder<TSignature> WithBody<TDeps>(TDeps deps, Func<TDeps, TSignature> bodyImpl);
    public ISyntheticMethodBuilder<TSignature> WithBody(ISyntheticCodeBlock body);

    public ISyntheticMethodBuilder<TSignature> WithAccessibility(AccessModifierList modifiers);

    /// <summary>
    /// API Surface required by the compiler implementation to declare synthetic components.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    [Experimental(Diagnostics.InternalFeatureId)]
    public interface λIInternalMethodBuilderCompilerApi {
        public ISyntheticCodeBlock CreateRendererCodeBlock(RenderFragment renderCodeBlock);
        public ISyntheticCodeBlock CreateRendererCodeBlock<TState>(RenderFragment<TState> renderCodeBlock);
    }

    private sealed class QtSyntheticMethodBuilderCompilerApi : λIInternalMethodBuilderCompilerApi {
        public ISyntheticCodeBlock CreateRendererCodeBlock(RenderFragment renderCodeBlock) {
            return new QtSyntheticRenderCodeBlock(renderCodeBlock);
        }

        public ISyntheticCodeBlock CreateRendererCodeBlock<TState>(RenderFragment<TState> renderCodeBlock) {
            return new QtSyntheticRenderCodeBlock<TState>(renderCodeBlock);
        }
    }
}