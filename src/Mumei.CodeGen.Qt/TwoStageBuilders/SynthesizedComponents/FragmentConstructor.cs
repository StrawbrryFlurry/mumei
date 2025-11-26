using Mumei.CodeGen.Qt.TwoStageBuilders.Components;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

internal static class FragmentConstructor {
    public static T? ConstructOptionalFragment<T>(ISyntheticCompilation compilation, object? maybeConstructable) {
        if (maybeConstructable is ISyntheticConstructable<T> constructable) {
            return constructable.Construct(compilation);
        }

        return default;
    }

    public static T? ConstructFragment<T>(ISyntheticCompilation compilation, object? maybeConstructable, T? defaultValue = default) {
        if (maybeConstructable is ISyntheticConstructable<T> constructable) {
            return constructable.Construct(compilation);
        }

        if (!defaultValue?.Equals(default(T)) ?? true) {
            return defaultValue;
        }

        throw new NotSupportedException();
    }
}