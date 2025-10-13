namespace Mumei.CodeGen.Qt;

internal sealed class FeatureCollection {
    private HashSet<ISourceFileFeature>? _sourceFileFeatures;
    private HashSet<ICompilationUnitFeature>? _compilationUnitFeatures;

    public void Require(ISourceFileFeature feature) {
        (_sourceFileFeatures ??= []).Add(feature);
    }

    public void Require(IRenderer.IFeature renderFeature) {
        throw new NotSupportedException();
    }

    public void Require(ICompilationUnitFeature feature) {
        (_compilationUnitFeatures ??= []).Add(feature);
    }
}