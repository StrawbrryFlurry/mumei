using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Playground;

public static partial class Impl {
    public static void ImplAsync() {
        var i = (ProxyAsyncMethodStateInfo i) => {
            var λsourceCtLinkedWithAssertionContext = CancellationTokenSource.CreateLinkedTokenSource(i.CancellationToken);
            var λsubject = new AsyncDelegateAssertionSubject<Task<Arg.TReturn>>(
                λcancellationToken => i.InvokeAsync(λcancellationToken),
                async λcancellationToken => await i.InvokeAsync(λcancellationToken)
            ) {
                Method = i.Method,
                Arguments = i.InvocationArguments,
                Receiver = i.This,
                SupportsCancellation = i.SupportsCancellation,
                CancellationTokenSource = λsourceCtLinkedWithAssertionContext,
                OriginalCancellationToken = i.CancellationToken
            };

            Assertion.Start(
                λsubject,
                i.Apply(new MinimalExpression<InvocationExpressionSyntax>()),
                i.Apply(new FullExpression<InvocationExpressionSyntax>())
            );
        };
    }
}

internal class ProxyAsyncMethodStateInfo : IQtSyntaxProvider<InvocationExpressionSyntax> {
    public InvocationExpressionSyntax Syntax { get; }

    public Task<Arg.TReturn> InvokeAsync(CancellationToken cancellationToken = default) {
        return default;
    }

    public Arg.TThis This { get; set; }
    public object[] InvocationArguments { get; set; }
    public MethodInfo Method { get; set; }

    public CancellationToken CancellationToken { get; set; }
    public bool SupportsCancellation { get; set; }

    public TR Apply<TR>(IQtSyntaxFunctor<InvocationExpressionSyntax, TR> fc) {
        return default;
    }
}

public readonly struct MinimalExpression<TSyntax> : IQtSyntaxFunctor<TSyntax, string> where TSyntax : SyntaxNode {
    public static QtSyntaxTemplateItem<string> Invoke(IQtSyntaxProvider<TSyntax> provider) {
        return TemplateResult.String(provider.Syntax.ToString());
    }
}

public readonly struct FullExpression<TSyntax> : IQtSyntaxFunctor<TSyntax, string> where TSyntax : SyntaxNode {
    public static QtSyntaxTemplateItem<string> Invoke(IQtSyntaxProvider<TSyntax> provider) {
        var parentStatement = provider.Syntax.FirstAncestorOrSelf<StatementSyntax>();
        return TemplateResult.String(parentStatement!.ToString());
    }
}

file static class λActuallyInterceptor_ValueAssertionTests_Task_Cancellation {
    [InterceptsLocation(1, "HX24xgy53kbO8JQb+nhp8Q0PAABWYWx1ZUFzc2VydGlvblRlc3RzLmNz")]
    public static Task<string> SetAsyncDelegateInvocationSubjectContext_AsyncMethodWithCancellation_0(
        this InterceptorMethodExample λthis,
        CancellationToken cancellationToken
    ) {
        var λoriginalCancellationToken = cancellationToken;
        var λsourceCtLinkedWithAssertionContext = CancellationTokenSource.CreateLinkedTokenSource(λoriginalCancellationToken);
        var λsubject = new AsyncDelegateAssertionSubject<Task<string>>(
            λcancellationToken => λthis.AsyncMethodWithCancellation(λcancellationToken),
            async λcancellationToken => await λthis.AsyncMethodWithCancellation(λcancellationToken)) {
            Method = typeof(InterceptorMethodExample).GetMethod("AsyncMethodWithCancellation")!,
            Arguments = new object[] {
                cancellationToken
            },
            Receiver = λthis,
            SupportsCancellation = true,
            CancellationTokenSource = λsourceCtLinkedWithAssertionContext,
            OriginalCancellationToken = λoriginalCancellationToken
        };

        Assertion.Start(λsubject,
            """
            AsyncMethodWithCancellation(CancellationToken.None)
            """,
            """
            AsyncMethodWithCancellation(CancellationToken.None)
            """
        );

        return Task.FromResult<string>(null!);
    }
}

public static partial class Impl {
    public static void Impl1() {
        var i = () => (ProxyMethodStateInfo<bool> i) => {
            var λactually = default(bool);
            try {
                λactually = i.Invoke();
            }
            catch (ActuallyAssertionFailedException innerAssertionFailedException) {
                AssertionScope.Failure(innerAssertionFailedException);
            }
            catch (Exception λex) {
                ParamitaAssert.FailBooleanInvocation(
                    ExceptionDispatchInfo.Capture(λex),
                    i.This,
                    i.Method,
                    i.InvocationArguments,
                    """""""""""""""
                    sut.Actually().SequenceEqual([1, 3, 3])
                    """"""""""""""",
                    """""""""""""""
                    Subject.SequenceEqual([1, 3, 3])
                    """""""""""""""
                );
            }
        };
    }
}

internal class ProxyMethodStateInfo<T> {
    public T Invoke() {
        return default;
    }

    public Arg.TThis This { get; set; }
    public object[] InvocationArguments { get; set; }
    public MethodInfo Method { get; set; }
}

file static class λActuallyInterceptor_EnumerableExtensionsTests_SequenceEquals_Array_SameOrder_Passes {
    [InterceptsLocation(1, "zVsGxYiARaQmeOOh9DtgwAMBAABFbnVtZXJhYmxlRXh0ZW5zaW9uc1Rlc3RzLmNz")]
    public static bool Assert_SequenceEqual_0(this IEnumerable<int> λthis, IEnumerable<int> second) {
        var λactually = default(bool);
        try {
            λactually = Enumerable.SequenceEqual(λthis, second);
        }
        catch (ActuallyAssertionFailedException innerAssertionFailedException) {
            AssertionScope.Failure(innerAssertionFailedException);
        }
        catch (Exception λex) {
            ParamitaAssert.FailBooleanInvocation(
                ExceptionDispatchInfo.Capture(λex),
                λthis,
                MethodReflectionExtensions.GetMethod(
                    typeof(Enumerable),
                    "SequenceEqual",
                    [typeof(int)],
                    [typeof(IEnumerable<int>), typeof(IEnumerable<int>)]
                ),
                [second],
                """""""""""""""
                sut.Actually().SequenceEqual([1, 3, 3])
                """"""""""""""",
                """""""""""""""
                Subject.SequenceEqual([1, 3, 3])
                """""""""""""""
            );
        }

        ParamitaAssert.BooleanInvocation(
            λactually,
            λthis,
            MethodReflectionExtensions.GetMethod(
                typeof(Enumerable),
                "SequenceEqual",
                [typeof(int)],
                [typeof(IEnumerable<int>), typeof(IEnumerable<int>)]
            ),
            [second],
            """""""""""""""
            sut.Actually().SequenceEqual([1, 3, 3])
            """"""""""""""",
            """""""""""""""
            Subject.SequenceEqual([1, 3, 3])
            """""""""""""""
        );
        return λactually;
    }

    [InterceptsLocation(1, "zVsGxYiARaQmeOOh9DtgwPgAAABFbnVtZXJhYmxlRXh0ZW5zaW9uc1Rlc3RzLmNz")]
    public static int[] SetObjectSubjectContext_Actually_1(this int[] λthis, string because) {
        var λsubject = new ValueAssertionSubject<int[]>(λthis);
        Assertion.Start(
            λsubject,
            """""""""""""""
            Subject
            """"""""""""""",
            """""""""""""""
            sut.Actually().SequenceEqual([1, 3, 3])
            """""""""""""""
        );
        return Unsafe.As<int[]>(λsubject);
    }
}

internal class MethodReflectionExtensions {
    public static object GetMethod(Type p0, string sequenceequal, Type[] p2, Type[] p3) {
        throw new NotImplementedException();
    }
}

internal class AssertionScope {
    public static void Failure(object result) {
        throw new NotImplementedException();
    }
}

internal class ActuallyAssertionFailedException : Exception { }

internal static class ParamitaAssert {
    public static void BooleanInvocation(bool λactually, object λthis, object getMethod, object[] objects, string sutActuallySequenceequal,
        string subjectSequenceequal) {
        throw new NotImplementedException();
    }

    public static void FailBooleanInvocation(ExceptionDispatchInfo capture, object λthis, object getMethod, object[] objects,
        string sutActuallySequenceequal, string subjectSequenceequal) {
        throw new NotImplementedException();
    }
}

internal sealed class ValueAssertionSubject<T> {
    public ValueAssertionSubject(int[] λthis) {
        throw new NotImplementedException();
    }
}

public sealed class InterceptorMethodExample {
    public Task<string> AsyncMethodWithCancellation(CancellationToken λcancellationToken) {
        throw new NotImplementedException();
    }
}

public static class Assertion {
    public static void Start(
        object subject,
        string minimalExpression,
        string fullExpression
    ) {
        throw new NotImplementedException();
    }
}

public sealed class AsyncDelegateAssertionSubject<TResult>(
    // Don't hardcode Task since ValueTask etc. are a thing
    Func<CancellationToken, TResult> invokeSubjectAsync,
    Func<CancellationToken, Task> invokeSubjectAsTaskAsync
) {
    public required MethodInfo Method { get; init; }
    public required object[] Arguments { get; init; }
    public required object Receiver { get; init; }

    public required bool SupportsCancellation { get; init; }

    // Proxy a custom cancellation token into the delegate invocation
    public required CancellationTokenSource CancellationTokenSource { get; init; }

    // The original cancellation token this async method was invoked with (if any)
    public required CancellationToken OriginalCancellationToken { get; init; }

    public Task InvokeSubjectAsTaskAsync(CancellationToken ct = default) {
        return invokeSubjectAsTaskAsync(ct);
    }

    public TResult InvokeSubjectAsync(CancellationToken ct = default) {
        return invokeSubjectAsync(ct);
    }

    public void Format(StringBuilder message) {
        message.Append("");
    }
}

internal class InterceptsLocationAttribute : Attribute {
    public InterceptsLocationAttribute(int i, string x) {
        throw new NotImplementedException();
    }
}

# region NonProxy

internal class InjectorImpl { }

#endregion