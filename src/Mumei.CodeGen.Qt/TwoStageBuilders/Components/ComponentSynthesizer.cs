using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class ComponentSynthesizer {
    public ComponentSynthesizer(Compilation compilation) { }

    public SynthesizedClassDeclaration Synthesize(IClassComponent classComponent) {
        return default;
    }
}