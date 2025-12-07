namespace Mumei.CodeGen.Components;

public interface ISyntheticFieldBuilder<T> : ISyntheticField<T> {
    public ISyntheticFieldBuilder<T> WithAccessibility(AccessModifierList modifiers);
}

public interface ISyntheticField { }
public interface ISyntheticField<T> : ISyntheticField { }