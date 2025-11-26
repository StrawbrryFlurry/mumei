using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public abstract class SyntheticMethodDefinition {
    public virtual void BindDynamicComponents(BindingContext ctx) { }

    public abstract ISyntheticCodeBlock GenerateMethodBody();

    public readonly struct BindingContext {
        public void Bind<T>(ISyntheticType type) { }
        public void Bind<T>(ITypeSymbol type) { }
        public void Bind<T>(Type type) { }
    }
}