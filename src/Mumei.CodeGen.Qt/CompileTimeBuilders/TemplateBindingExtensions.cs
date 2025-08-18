using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

public static class TemplateBindingExtensions {
    /// <summary>
    /// Evaluates a boolean value at compile time, allowing for different
    /// specialized behavior to be generated based on input conditions.
    /// This method can only be used to evaluate template references passed
    /// to the template block
    /// <code>
    /// Fragment(new { elements }, ({ elements }) => {
    ///   if ((elements.Length > 0).TemplateEvaluate()) {
    ///
    ///   } else {
    ///     Console.WriteLine("No elements found.");
    ///   }
    /// });
    /// </code>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="CompileTimeComponentUsedAtRuntimeException"></exception>
    public static bool TemplateEvaluate(this bool value) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static TProjection TemplateRealize<TSource, TProjection>(
        this IEnumerable<TSource> source,
        Func<TSource, TProjection> projection
    ) where TProjection : IQtTemplateBindable {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}