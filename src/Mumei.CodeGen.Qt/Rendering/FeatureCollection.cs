namespace Mumei.CodeGen.Qt;

internal sealed class FeatureCollection {
    private HashSet<ISourceFileFeature>? _sourceFileFeatures;
    private HashSet<ICompilationFeature>? _compilationUnitFeatures;

    public void Require(ISourceFileFeature feature) {
        (_sourceFileFeatures ??= []).Add(feature);
    }

    public void Require(IRenderer.IFeature renderFeature) {
        if (renderFeature is ISourceFileFeature sourceFileFeature) {
            Require(sourceFileFeature);
            return;
        }

        if (renderFeature is ICompilationFeature compilationFeature) {
            Require(compilationFeature);
            return;
        }

        throw new NotSupportedException();
    }

    public void Require(ICompilationFeature feature) {
        (_compilationUnitFeatures ??= []).Add(feature);
    }

    public void RenderSourceFileFeatures(IRenderTreeBuilder renderTree) {
        if (_sourceFileFeatures is null) {
            return;
        }

        var i = _sourceFileFeatures.Count;
        foreach (var feature in _sourceFileFeatures) {
            renderTree.Node(feature);
            if (--i != 0) {
                renderTree.NewLine();
            }
        }
    }
}