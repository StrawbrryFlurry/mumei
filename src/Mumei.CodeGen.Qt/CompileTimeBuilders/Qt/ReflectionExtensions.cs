using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Qt.Qt;

public static class MethodReflectionExtensions {
    public static MethodInfo GetMethod(
        Type declaringType,
        string methodName,
        Type[] typeParameters,
        Type[] parameterTypes
    ) {
        var methods = declaringType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
        if (typeParameters.Length != 0) {
            return GetMethodCore<GenericMatchingStrategy>(methodName, typeParameters, parameterTypes, methods);
        }

        return GetMethodCore<NonGenericMatchingStrategy>(methodName, typeParameters, parameterTypes, methods);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MethodInfo GetMethodCore<TMatchingStrategy>(
        string methodName,
        Type[] typeParameters,
        Type[] parameterTypes,
        MethodInfo[] methods
    ) where TMatchingStrategy : struct, IMatchingStrategy {
        foreach (var method in methods) {
            if (MatchMethodCore(method, methodName, typeParameters, parameterTypes) is not { } matchedMethod) {
                continue;
            }

            if (typeof(TMatchingStrategy) == typeof(GenericMatchingStrategy) && matchedMethod.IsGenericMethodDefinition) {
                matchedMethod = matchedMethod.MakeGenericMethod(typeParameters);
            }

            return matchedMethod;
        }

        throw new MissingMethodException($"Method {methodName} with {typeParameters} type parameters and {parameterTypes.Length} parameters not found.");

        static MethodInfo? MatchMethodCore(
            MethodInfo method,
            string methodName,
            Type[] typeParameters,
            Type[] parameterTypes
        ) {
            if (method.Name != methodName) {
                return null;
            }

            var isGenericMethod = method.IsGenericMethodDefinition;
            if (typeof(TMatchingStrategy) == typeof(NonGenericMatchingStrategy) && method.IsGenericMethodDefinition) {
                return null;
            }

            if (typeof(TMatchingStrategy) == typeof(GenericMatchingStrategy) && isGenericMethod) {
                var genericArguments = method.GetGenericArguments();
                if (genericArguments.Length != typeParameters.Length) {
                    return null;
                }
            }

            var parameters = method.GetParameters();
            if (parameters.Length != parameterTypes.Length) {
                return null;
            }

            var match = method;
            for (var i = 0; i < parameters.Length; i++) {
                var sourceType = parameters[i].ParameterType;
                var targetType = parameterTypes[i];
                if (sourceType == targetType) {
                    continue;
                }

                if (typeof(TMatchingStrategy) != typeof(GenericMatchingStrategy)) {
                    continue;
                }

                if (
                    sourceType.IsGenericType
                    && targetType.IsGenericType
                    && sourceType.GetGenericTypeDefinition() == targetType.GetGenericTypeDefinition()
                ) {
                    continue;
                }

                match = null;
                break;
            }

            return match;
        }
    }

    private interface IMatchingStrategy;

    private readonly struct NonGenericMatchingStrategy : IMatchingStrategy;

    private readonly struct GenericMatchingStrategy : IMatchingStrategy;
}