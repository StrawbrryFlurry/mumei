using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

/// <summary>
/// Defines a type in the synthetic type system that represents any user created type
/// or a reference to a runtime or compile-time type.
/// </summary>
public interface ISyntheticType {
    public SyntheticIdentifier Name { get; }

    public bool Equals(ISyntheticType other);
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
    public SyntheticIdentifier Name { get; } = typeName ?? $"<error: {errorReason}>";

    public object UnderlyingType { get; }

    public bool Equals(ISyntheticType other) {
        if (other is ErrorSyntheticType otherErrorType) {
            return Name.Equals(otherErrorType.Name);
        }

        return false;
    }
}

internal sealed class RuntimeSyntheticType(Type t) : ISyntheticType, ISyntheticConstructable<TypeInfoFragment> {
    public Type UnderlyingType => t;

    public SyntheticIdentifier Name => t.Name;

    public bool Equals(ISyntheticType other) {
        if (other is RuntimeSyntheticType otherRuntimeType) {
            return otherRuntimeType.UnderlyingType == t;
        }

        return other.Equals(this); // Delegate to the other type's equality implementation
    }

    public TypeInfoFragment Construct(ICompilationUnitContext compilationUnit) {
        return new TypeInfoFragment(t);
    }
}

internal sealed class QtSyntheticTypeInfo<T> : ISyntheticTypeInfo<T> {
    public T New(params object[] args) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T New(Func<T> constructorExpression) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}