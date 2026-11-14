using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Roslyn;

[Union]
public readonly record struct GeneratorStepResult<T> {
    private readonly byte _kind;

    private readonly T? _result;
    private readonly EquatableArray<Diagnostic> _diagnostics;

    public object? Value => throw new NotSupportedException();

    public GeneratorStepResult(Candidate<T> candidate) {
        _kind = 1;
        _result = candidate.Result;
    }

    public GeneratorStepResult(SourceError error) {
        _kind = 2;
        _diagnostics = error.Diagnostics;
    }

    public GeneratorStepResult(NoMatch _) {
        _kind = 3;
    }

    public bool TryGetValue(out Candidate<T> candidate) {
        candidate = new Candidate<T>(_result!);
        return _kind == 1;
    }

    public bool TryGetValue(out SourceError errors) {
        errors = new SourceError(_diagnostics);
        return _kind == 2;
    }

    public bool TryGetValue(out NoMatch noMatch) {
        noMatch = default;
        return _kind == 3;
    }
}

public readonly record struct Candidate<T>(T Result);

public readonly record struct SourceError(ImmutableArray<Diagnostic> Diagnostics);

public readonly record struct NoMatch;

[Union]
public readonly record struct Maybe<T> {
    private readonly byte _kind = 0;
    private readonly T? _value;

    public object? Value => _value;

    public Maybe(T something) {
        _kind = 1;
        _value = something;
    }

    public Maybe(Something<T> something) {
        _kind = 1;
        _value = something.Value;
    }

    public Maybe(Nothing _) { }

    public bool TryGetValue(out Something<T> value) {
        value = new Something<T>(_value!);
        return _kind == 1;
    }

    public bool TryGetValue(out Nothing value) {
        value = default;
        return _kind == 0;
    }
}

public static class Maybe {
    public static Maybe<T> Of<T>(T value) {
        return new Maybe<T>(value);
    }
}

public readonly record struct Something<T>(T Value);

public readonly record struct Nothing;