using System.Diagnostics.CodeAnalysis;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticMethodBuilder<TSignature> : ISyntheticMethod<TSignature> where TSignature : Delegate {
    [Experimental(Diagnostics.InternalFeatureId)]
    public λIInternalMethodBuilderCompilerApi λCompilerApi { get; }

    public ISyntheticMethodBuilder<TSignature> WithBody(TSignature bodyImpl);
    public ISyntheticMethodBuilder<TSignature> WithBody<TDeps>(TDeps deps, Func<TDeps, TSignature> bodyImpl);
    public ISyntheticMethodBuilder<TSignature> WithBody(ISyntheticCodeBlock body);

    public ISyntheticMethodBuilder<TSignature> WithAccessibility(SyntheticAccessModifier modifiers);

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
            return new SyntheticRenderCodeBlock(renderCodeBlock);
        }

        public ISyntheticCodeBlock CreateRendererCodeBlock<TState>(RenderFragment<TState> renderCodeBlock) {
            return new SyntheticRenderCodeBlock<TState>(renderCodeBlock);
        }
    }
}

internal sealed class QtSyntheticMethodBuilder<TSignature> : ISyntheticMethodBuilder<TSignature> where TSignature : Delegate {
    ISyntheticMethodBuilder<TSignature>.λIInternalMethodBuilderCompilerApi ISyntheticMethodBuilder<TSignature>.λCompilerApi { get; }

    public string Name { get; }

    public TSignature Bind(object target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TSignature1 BindAs<TSignature1>(object target) where TSignature1 : Delegate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticMethodBuilder<TSignature> WithBody(TSignature bodyImpl) {
        throw new NotImplementedException();
    }
    public ISyntheticMethodBuilder<TSignature> WithBody<TDeps>(TDeps deps, Func<TDeps, TSignature> bodyImpl) {
        throw new NotImplementedException();
    }
    public ISyntheticMethodBuilder<TSignature> WithBody(ISyntheticCodeBlock body) {
        throw new NotImplementedException();
    }

    public ISyntheticMethodBuilder<TSignature> WithAccessibility(SyntheticAccessModifier modifiers) {
        throw new NotImplementedException();
    }

    private sealed class CompilerApi : ISyntheticMethodBuilder<TSignature>.λIInternalMethodBuilderCompilerApi {
        public ISyntheticCodeBlock CreateRendererCodeBlock(RenderFragment renderCodeBlock) {
            throw new NotImplementedException();
        }

        public ISyntheticCodeBlock CreateRendererCodeBlock<TState>(RenderFragment<TState> renderCodeBlock) {
            throw new NotImplementedException();
        }
    }
}