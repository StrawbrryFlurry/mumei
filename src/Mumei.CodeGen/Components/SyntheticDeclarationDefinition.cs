using System.Runtime.CompilerServices;
using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen.Components;

public abstract class SyntheticDeclarationDefinition {
    private Dictionary<string, ISyntheticType>? _lateBoundTypeBindings;

    protected internal ICodeGenerationContext CodeGenContext { get; internal set; } = null!;

    // ReSharper disable once EntityNameCapturedOnly.Global
    public void Bind(Type lateBoundType, ISyntheticType actualType, [CallerArgumentExpression(nameof(lateBoundType))] string lateBoundTypeExpression = "") {
        _lateBoundTypeBindings ??= new Dictionary<string, ISyntheticType>();
        var typeExpression = lateBoundTypeExpression.AsSpan();
        if (!typeExpression.StartsWith("typeof(") || lateBoundTypeExpression[^1] != ')') {
            throw new InvalidOperationException("Late bound type expression must be of the form 'typeof(TLateBoundType)'.");
        }

        var lateBoundTypeName = typeExpression["typeof(".Length..^1].ToString();
        _lateBoundTypeBindings[lateBoundTypeName] = actualType;
    }

    public static void Emit<TFragment>(TFragment fragment) where TFragment : IRenderFragment {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static T EmitExpression<T>(IRenderFragment fragment) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static IEnumerable<T> EmitForEach<T>(IEnumerable<T> elements) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static bool EmitCondition(bool condition) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    protected internal ISyntheticType InternalResolveLateBoundType(string lateBoundTypeName) {
        if (_lateBoundTypeBindings is null || !_lateBoundTypeBindings.TryGetValue(lateBoundTypeName, out var result)) {
            throw new InvalidOperationException($"No type was bound to {lateBoundTypeName}. Use definition.{nameof(Bind)} to bind a type.");
        }

        return result;
    }
}