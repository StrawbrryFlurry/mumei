using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public static class QtBindableExtensions {
    public static IQtParameter Parameter(this Type type, string? name = null) {
        return null!;
    }

    public static IQtParameter Out<T>(this T bindable) where T : IQtTemplateBindable {
        return null!;
    }

    public static IQtParameter WithDefault<T>(this T bindable, object defaultValue) where T : IQtTemplateBindable {
        return null!;
    }
}