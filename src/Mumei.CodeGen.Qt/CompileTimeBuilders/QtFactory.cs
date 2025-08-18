using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

public static class QtFactory {
    /// <summary>
    /// Creates a new <see cref="QtFragment"/> using the provided template references and fragment descriptor.
    /// </summary>
    /// <param name="refs"></param>
    /// <param name="fragmentDescriptor"></param>
    /// <typeparam name="TTemplateReferences"></typeparam>
    /// <returns></returns>
    public static QtFragment Fragment<TTemplateReferences>(
        TTemplateReferences refs,
        Action<TTemplateReferences> fragmentDescriptor
    ) where TTemplateReferences : IQtTemplateBindable {
        return default;
    }

    public static QtTemplate Template(string template) {
        return default;
    }

    public static QtTemplate TemplateFromClass<TClass>() {
        return default;
    }

    public static QtNamespace Namespace() {
        return new QtNamespace();
    }
}