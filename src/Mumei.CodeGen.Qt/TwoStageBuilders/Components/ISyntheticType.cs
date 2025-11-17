namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

/// <summary>
/// Defines a type in the synthetic type system that represents any user created type
/// or a reference to a runtime or compile-time type.
/// </summary>
public interface ISyntheticType {
    public string Name { get; }
}

/// <summary>
/// Defines a type that the synthetic type system can create instances of.
/// For now, this is deliberately separated from <see cref="ISyntheticType"/>
/// since <see cref="ISyntheticTypeInfo{T}"/> requires us to know the target type at compile-time whereas
/// <see cref="ISyntheticType"/> may point to any type, including those only known at runtime.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISyntheticTypeInfo<T> {
    public T New(params object[] args);
    public T New(Func<T> constructorExpression);
}

internal sealed class ErrorSyntheticType(string? typeName, string errorReason, object declarationSite) : ISyntheticType {
    public string Name { get; } = typeName ?? $"<error: {errorReason}>";
}

internal sealed class RuntimeSyntheticType(Type t) : ISyntheticType {
    public string Name => t.Name;
}

internal sealed class QtSyntheticTypeInfo<T> : ISyntheticTypeInfo<T> {
    public T New(params object[] args) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T New(Func<T> constructorExpression) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}