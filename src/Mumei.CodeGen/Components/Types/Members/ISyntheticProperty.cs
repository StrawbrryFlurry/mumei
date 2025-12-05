namespace Mumei.CodeGen.Components;

public interface ISyntheticPropertyBuilder<T> : ISyntheticProperty<T> {
    public ISyntheticPropertyBuilder<T> WithAccessibility(AccessModifierList modifiers);
};

public interface ISyntheticProperty<T> : ISyntheticProperty;
public interface ISyntheticProperty;