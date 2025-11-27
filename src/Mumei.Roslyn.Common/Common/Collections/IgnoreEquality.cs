namespace Mumei.Roslyn;

public readonly struct IgnoreEquality<T>(T value) : IEquatable<IgnoreEquality<T>> {
    public T Value { get; } = value;

    public static implicit operator T(IgnoreEquality<T> wrapper) {
        return wrapper.Value;
    }

    public static implicit operator IgnoreEquality<T>(T value) {
        return new IgnoreEquality<T>(value);
    }

    public bool Equals(IgnoreEquality<T> other) {
        return true;
    }

    public override bool Equals(object? obj) {
        return obj is IgnoreEquality<T> other && Equals(other);
    }

    public override int GetHashCode() {
        return Value?.GetHashCode() ?? 0;
    }
}