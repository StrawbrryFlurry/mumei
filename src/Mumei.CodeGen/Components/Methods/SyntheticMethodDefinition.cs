using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Components;

public abstract class SyntheticMethodDefinition {
    public virtual void BindDynamicComponents(BindingContext ctx) { }

    public virtual void InternalBindCompilerMethod(
        ISimpleSyntheticClassBuilder builder,
        BindingContext bindingContext,
        Delegate targetMethod
    ) {
        throw new InvalidOperationException("Method body generation not implemented.");
    }

    public readonly struct BindingContext() {
        private readonly Dictionary<string, ISyntheticType> _dynamicallyBoundTypes = new();
        public ISyntheticType ResolveDynamicallyBoundType(string name) {
            if (_dynamicallyBoundTypes.TryGetValue(name, out var result)) {
                return result;
            }

            throw new InvalidOperationException($"Dynamically bound type '{name}' not found.");
        }

        public void Bind(Type targetType, ISyntheticType type, [CallerArgumentExpression(nameof(targetType))] string targetTypeExpression = "") {
            var typeName = targetTypeExpression.Substring("typeof(".Length, targetTypeExpression.Length - "typeof(".Length - 1);
            _dynamicallyBoundTypes[typeName] = type;
        }
        public void Bind(Type targetType, Type type, [CallerArgumentExpression(nameof(targetType))] string targetTypeExpression = "") { }
    }
}