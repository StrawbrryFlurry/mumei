using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Components;

public abstract class SyntheticMethodDefinition : ISyntheticMethodDefinition {
    public virtual void BindDynamicComponents(MethodDefinitionBindingContext ctx) { }

    public virtual ISyntheticMethodBuilder<Delegate> InternalBindCompilerMethod(
        ISimpleSyntheticClassBuilder builder,
        MethodDefinitionBindingContext bindingContext,
        Delegate targetMethod
    ) {
        throw new InvalidOperationException("Method body generation not implemented.");
    }
}

public readonly struct MethodDefinitionBindingContext() {
    private readonly Dictionary<string, ISyntheticType> _dynamicallyBoundTypes = new();
    public ISyntheticType ResolveDynamicallyBoundType(string name) {
        if (_dynamicallyBoundTypes.TryGetValue(name, out var result)) {
            return result;
        }

        throw new InvalidOperationException($"Dynamically bound type '{name}' not found.");
    }

    // ReSharper disable once EntityNameCapturedOnly.Global
    public void Bind(Type targetType, ISyntheticType type, [CallerArgumentExpression(nameof(targetType))] string targetTypeExpression = "") {
        var typeName = targetTypeExpression.Substring("typeof(".Length, targetTypeExpression.Length - "typeof(".Length - 1);
        _dynamicallyBoundTypes[typeName] = type;
    }

    // ReSharper disable once EntityNameCapturedOnly.Global
    public void Bind(Type targetType, Type type, [CallerArgumentExpression(nameof(targetType))] string targetTypeExpression = "") { }
}