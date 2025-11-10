using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.Tests.Setup;

internal static class DynamicCompilationAssemblyAssertions {
    public static void PassesAssemblyAction(
        this SourceGeneratorTestResult runResult,
        Action<GeneratedAssembly> assemblyAction
    ) {
        var inMemoryAssemblyStream = new MemoryStream();
        runResult.Compilation.Emit(inMemoryAssemblyStream);
        var alc = new AssemblyLoadContext($"DynamicCompilationAssemblyAssertions_{RandomNumberGenerator.GetHexString(16)}", true);
        inMemoryAssemblyStream.Position = 0; // Start reading from the beginning
        var assembly = alc.LoadFromStream(inMemoryAssemblyStream);

        var generatedAssembly = new GeneratedAssembly(assembly);
        assemblyAction(generatedAssembly);
        alc.Unload();
    }

    public sealed class GeneratedAssembly(Assembly assembly) {
        public Assembly Assembly { get; } = assembly;
        public GeneratedAssemblyInstance<T> CreateInstance<T>(params object[] args) {
            var name = GetTypeNameWithoutFileScope(typeof(T));
            var instance = Activator.CreateInstance(Assembly.GetTypes().First(x => x.FullName == name), args)
                           ?? throw new InvalidOperationException("Could not create instance of type " + name);
            // We can't cast here since the type is from a different assembly load context.
            return new GeneratedAssemblyInstance<T>(Unsafe.As<object, T>(ref instance), this);
        }

        public TResult Invoke<T, TResult>(GeneratedAssemblyInstance<T> instance, Expression<Func<T, TResult>> expression) {
            var (methodInfo, args) = InvocationExpressionVisitor.FindInvocation(expression.Body)
                                     ?? throw new InvalidOperationException("The provided expression does not contain an invocation.");

            return InvokeCore<T, TResult>(instance.UnsafeValue, methodInfo, args);
        }

        public void Invoke<T>(GeneratedAssemblyInstance<T> instance, Expression<Action<T>> expression) {
            var (methodInfo, args) = InvocationExpressionVisitor.FindInvocation(expression.Body)
                                     ?? throw new InvalidOperationException("The provided expression does not contain an invocation.");

            InvokeCore<T, Unit>(instance.UnsafeValue, methodInfo, args);
        }

        private TResult InvokeCore<T, TResult>(T instance, MethodInfo method, object?[] args) {
            // We can't directly invoke the method info since the method we got
            // is from the calling assembly's context. The instance's type (as far as the compiler is concerned)
            // is from the reinterpret-casted type that we created, and invoking a method on it will call the implementation
            // in the calling assembly, not the generated one.
            var actualMethod = FindGeneratedAssemblyMethodInfoImplementation(instance, method);
            var result = actualMethod.Invoke(instance, args);
            return Unsafe.As<object?, TResult>(ref result);
        }

        private MethodInfo FindGeneratedAssemblyMethodInfoImplementation<TCallingAssemblyType>(TCallingAssemblyType instance, MethodInfo methodInfo) {
            var methodInfos = instance!.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return methodInfos.FirstOrDefault(
                x => x.Name == methodInfo.Name
                     && x.GetParameters().Length == methodInfo.GetParameters().Length
                     && x.ReturnType.FullName == GetTypeNameWithoutFileScope(methodInfo.ReturnType)
                     && (methodInfo.IsGenericMethod == false || x.IsGenericMethod && x.GetGenericArguments().Length == methodInfo.GetGenericArguments().Length)
            ) ?? throw new InvalidOperationException("Could not find method " + methodInfo.Name + " on type " + instance!.GetType().FullName);
        }

        private sealed class InvocationExpressionVisitor : ExpressionVisitor {
            private (MethodInfo method, object?[] args)? _foundInvocation = null!;
            public static (MethodInfo method, object?[] args)? FindInvocation(Expression expression) {
                var visitor = new InvocationExpressionVisitor();
                visitor.Visit(expression);
                return visitor._foundInvocation;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node) {
                var args = node.Arguments.Select(EvaluateExpression).ToArray();
                _foundInvocation = (node.Method, args);
                return base.VisitMethodCall(node);
            }
        }

        private static string GetTypeNameWithoutFileScope(Type type) {
            var name = type.FullName!;
            if (name.IndexOf("<", StringComparison.Ordinal) == -1) {
                return name;
            }

            var fileScopeQualifier = name[name.IndexOf("<", StringComparison.Ordinal)..(name.LastIndexOf("_", StringComparison.Ordinal) + 1)];
            var nameWithoutFileScope = name.Replace(fileScopeQualifier, "");
            return nameWithoutFileScope;
        }
    }

    private static object EvaluateExpression(Expression expression) {
        if (expression is ConstantExpression constantExpression) {
            return constantExpression.Value!;
        }

        if (expression is MemberExpression memberExpression) {
            var lambda = Expression.Lambda(memberExpression);
            return lambda.Compile().DynamicInvoke()!;
        }

        var valueExtractor = Expression.Lambda<Func<object>>(
            Expression.Convert(expression, typeof(object))
        ).Compile();

        return valueExtractor();
    }

    public sealed class GeneratedAssemblyInstance<T>(T value, GeneratedAssembly assembly) {
        /// <summary>
        /// This is not actually an instance of <typeparamref name="T"/>, but instead an
        /// instance of <typeparamref name="T"/>, which was compiled from the same source code
        /// into the generator assembly. Calling methods on this instance directly will call the
        /// implementation in the calling assembly, not the generated one. Use <see cref="GeneratedAssembly.Invoke{T, TResult}"/> or
        /// <see cref="GeneratedAssembly.Invoke{T}"/> to invoke methods on this instance.
        /// </summary>
        public T UnsafeValue { get; } = value;

        public TResult Invoke<TResult>(Expression<Func<T, TResult>> expression) {
            return assembly.Invoke(this, expression);
        }

        public void Invoke(Expression<Action<T>> expression) {
            assembly.Invoke(this, expression);
        }
    }
}