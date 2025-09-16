using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.Misc;

public sealed class AnonymousStateClassLayoutTests {
    [Fact]
    public void GeneratedStateClass_MatchesAnonymousTypeLayout() {
        var state1 = new { Value1 = 1 };
        var state2 = new { Value1 = 1, Value2 = "2" };
        var state3 = new { Value1 = 1, Value2 = "2", Value3 = true };
        var state4 = new { Value1 = 1, Value2 = "2", Value3 = true, Value4 = 4.0 };
        var state5 = new { Value1 = 1, Value2 = "2", Value3 = true, Value4 = 4.0, Value5 = new StateArgRefValue(0, "", false) };
        var state6 = new { Value1 = 1, Value2 = "2", Value3 = true, Value4 = 4.0, Value5 = new StateArgRefValue(0, "", false), Value6 = new StateArgValue() };
        var state7 = new { Value1 = 1, Value2 = "2", Value3 = true, Value4 = 4.0, Value5 = new StateArgRefValue(0, "", false), Value6 = new StateArgValue(), Value7 = 6L };
        var state8 = new { Value1 = 1, Value2 = "2", Value3 = true, Value4 = 4.0, Value5 = new StateArgRefValue(0, "", false), Value6 = new StateArgValue(), Value7 = 6L, Value8 = (byte) 8 };
        var state9 = new { Value1 = 1, Value2 = "2", Value3 = true, Value4 = 4.0, Value5 = new StateArgRefValue(0, "", false), Value6 = new StateArgValue(), Value7 = 6L, Value8 = (byte) 8, Value9 = (short) 9 };
        var state10 = new { Value1 = 1, Value2 = "2", Value3 = true, Value4 = 4.0, Value5 = new StateArgRefValue(0, "", false), Value6 = new StateArgValue(), Value7 = 6L, Value8 = (byte) 8, Value9 = (short) 9, Value10 = 10u };
        var state11 = new { Value1 = 1, Value2 = "2", Value3 = true, Value4 = 4.0, Value5 = new StateArgRefValue(0, "", false), Value6 = new StateArgValue(), Value7 = 6L, Value8 = (byte) 8, Value9 = (short) 9, Value10 = 10u, Value11 = 11UL };
        var state12 = new { Value1 = 1, Value2 = "2", Value3 = true, Value4 = 4.0, Value5 = new StateArgRefValue(0, "", false), Value6 = new StateArgValue(), Value7 = 6L, Value8 = (byte) 8, Value9 = (short) 9, Value10 = 10u, Value11 = 11UL, Value12 = 'X' };

        var state1Accessor = Unsafe.As<StateAccessor<int>>(state1);
        Assert.Equal(state1Accessor.Value1, state1.Value1);

        var state2Accessor = Unsafe.As<StateAccessor<int, string>>(state2);
        Assert.Equal(state2Accessor.Value1, state2.Value1);
        Assert.Equal(state2Accessor.Value2, state2.Value2);

        var state3Accessor = Unsafe.As<StateAccessor<int, string, bool>>(state3);
        Assert.Equal(state3Accessor.Value1, state3.Value1);
        Assert.Equal(state3Accessor.Value2, state3.Value2);
        Assert.Equal(state3Accessor.Value3, state3.Value3);

        var state4Accessor = Unsafe.As<StateAccessor<int, string, bool, double>>(state4);
        Assert.Equal(state4Accessor.Value1, state4.Value1);
        Assert.Equal(state4Accessor.Value2, state4.Value2);
        Assert.Equal(state4Accessor.Value3, state4.Value3);
        Assert.Equal(state4Accessor.Value4, state4.Value4);

        var state5Accessor = Unsafe.As<StateAccessor<int, string, bool, double, StateArgRefValue>>(state5);
        Assert.Equal(state5Accessor.Value1, state5.Value1);
        Assert.Equal(state5Accessor.Value2, state5.Value2);
        Assert.Equal(state5Accessor.Value3, state5.Value3);
        Assert.Equal(state5Accessor.Value4, state5.Value4);
        Assert.Equal(state5Accessor.Value5, state5.Value5);

        var state6Accessor = Unsafe.As<StateAccessor<int, string, bool, double, StateArgRefValue, StateArgValue>>(state6);
        Assert.Equal(state6Accessor.Value1, state6.Value1);
        Assert.Equal(state6Accessor.Value2, state6.Value2);
        Assert.Equal(state6Accessor.Value3, state6.Value3);
        Assert.Equal(state6Accessor.Value4, state6.Value4);
        Assert.Equal(state6Accessor.Value5, state6.Value5);
        Assert.Equal(state6Accessor.Value6, state6.Value6);

        var state7Accessor = Unsafe.As<StateAccessor<int, string, bool, double, StateArgRefValue, StateArgValue, long>>(state7);
        Assert.Equal(state7Accessor.Value1, state7.Value1);
        Assert.Equal(state7Accessor.Value2, state7.Value2);
        Assert.Equal(state7Accessor.Value3, state7.Value3);
        Assert.Equal(state7Accessor.Value4, state7.Value4);
        Assert.Equal(state7Accessor.Value5, state7.Value5);
        Assert.Equal(state7Accessor.Value6, state7.Value6);
        Assert.Equal(state7Accessor.Value7, state7.Value7);

        var state8Accessor = Unsafe.As<StateAccessor<int, string, bool, double, StateArgRefValue, StateArgValue, long, byte>>(state8);
        Assert.Equal(state8Accessor.Value1, state8.Value1);
        Assert.Equal(state8Accessor.Value2, state8.Value2);
        Assert.Equal(state8Accessor.Value3, state8.Value3);
        Assert.Equal(state8Accessor.Value4, state8.Value4);
        Assert.Equal(state8Accessor.Value5, state8.Value5);
        Assert.Equal(state8Accessor.Value6, state8.Value6);
        Assert.Equal(state8Accessor.Value7, state8.Value7);
        Assert.Equal(state8Accessor.Value8, state8.Value8);

        var state9Accessor = Unsafe.As<StateAccessor<int, string, bool, double, StateArgRefValue, StateArgValue, long, byte, short>>(state9);
        Assert.Equal(state9Accessor.Value1, state9.Value1);
        Assert.Equal(state9Accessor.Value2, state9.Value2);
        Assert.Equal(state9Accessor.Value3, state9.Value3);
        Assert.Equal(state9Accessor.Value4, state9.Value4);
        Assert.Equal(state9Accessor.Value5, state9.Value5);
        Assert.Equal(state9Accessor.Value6, state9.Value6);
        Assert.Equal(state9Accessor.Value7, state9.Value7);
        Assert.Equal(state9Accessor.Value8, state9.Value8);
        Assert.Equal(state9Accessor.Value9, state9.Value9);

        var state10Accessor = Unsafe.As<StateAccessor<int, string, bool, double, StateArgRefValue, StateArgValue, long, byte, short, uint>>(state10);
        Assert.Equal(state10Accessor.Value1, state10.Value1);
        Assert.Equal(state10Accessor.Value2, state10.Value2);
        Assert.Equal(state10Accessor.Value3, state10.Value3);
        Assert.Equal(state10Accessor.Value4, state10.Value4);
        Assert.Equal(state10Accessor.Value5, state10.Value5);
        Assert.Equal(state10Accessor.Value6, state10.Value6);
        Assert.Equal(state10Accessor.Value7, state10.Value7);
        Assert.Equal(state10Accessor.Value8, state10.Value8);
        Assert.Equal(state10Accessor.Value9, state10.Value9);
        Assert.Equal(state10Accessor.Value10, state10.Value10);

        var state11Accessor = Unsafe.As<StateAccessor<int, string, bool, double, StateArgRefValue, StateArgValue, long, byte, short, uint, ulong>>(state11);
        Assert.Equal(state11Accessor.Value1, state11.Value1);
        Assert.Equal(state11Accessor.Value2, state11.Value2);
        Assert.Equal(state11Accessor.Value3, state11.Value3);
        Assert.Equal(state11Accessor.Value4, state11.Value4);
        Assert.Equal(state11Accessor.Value5, state11.Value5);
        Assert.Equal(state11Accessor.Value6, state11.Value6);
        Assert.Equal(state11Accessor.Value7, state11.Value7);
        Assert.Equal(state11Accessor.Value8, state11.Value8);
        Assert.Equal(state11Accessor.Value9, state11.Value9);
        Assert.Equal(state11Accessor.Value10, state11.Value10);
        Assert.Equal(state11Accessor.Value11, state11.Value11);

        var state12Accessor = Unsafe.As<StateAccessor<int, string, bool, double, StateArgRefValue, StateArgValue, long, byte, short, uint, ulong, char>>(state12);
        Assert.Equal(state12Accessor.Value1, state12.Value1);
        Assert.Equal(state12Accessor.Value2, state12.Value2);
        Assert.Equal(state12Accessor.Value3, state12.Value3);
        Assert.Equal(state12Accessor.Value4, state12.Value4);
        Assert.Equal(state12Accessor.Value5, state12.Value5);
        Assert.Equal(state12Accessor.Value6, state12.Value6);
        Assert.Equal(state12Accessor.Value7, state12.Value7);
        Assert.Equal(state12Accessor.Value8, state12.Value8);
        Assert.Equal(state12Accessor.Value9, state12.Value9);
        Assert.Equal(state12Accessor.Value10, state12.Value10);
        Assert.Equal(state12Accessor.Value11, state12.Value11);
        Assert.Equal(state12Accessor.Value12, state12.Value12);
    }
}

// ReSharper disable NotAccessedPositionalProperty.Local
file sealed record StateArgRefValue(int A, string B, bool C);
file readonly record struct StateArgValue(int A, string B, bool C);
#pragma warning disable

// ReSharper disable UnassignedGetOnlyAutoProperty
file sealed class StateAccessor<T1> {
    public T1 Value1 { get; }
}

file sealed class StateAccessor<T1, T2> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
}

file sealed class StateAccessor<T1, T2, T3> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
}

file sealed class StateAccessor<T1, T2, T3, T4> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
    public T4 Value4 { get; }
}

file sealed class StateAccessor<T1, T2, T3, T4, T5> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
    public T4 Value4 { get; }
    public T5 Value5 { get; }
}

file sealed class StateAccessor<T1, T2, T3, T4, T5, T6> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
    public T4 Value4 { get; }
    public T5 Value5 { get; }
    public T6 Value6 { get; }
}

file sealed class StateAccessor<T1, T2, T3, T4, T5, T6, T7> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
    public T4 Value4 { get; }
    public T5 Value5 { get; }
    public T6 Value6 { get; }
    public T7 Value7 { get; }
}

file sealed class StateAccessor<T1, T2, T3, T4, T5, T6, T7, T8> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
    public T4 Value4 { get; }
    public T5 Value5 { get; }
    public T6 Value6 { get; }
    public T7 Value7 { get; }
    public T8 Value8 { get; }
}

file sealed class StateAccessor<T1, T2, T3, T4, T5, T6, T7, T8, T9> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
    public T4 Value4 { get; }
    public T5 Value5 { get; }
    public T6 Value6 { get; }
    public T7 Value7 { get; }
    public T8 Value8 { get; }
    public T9 Value9 { get; }
}

file sealed class StateAccessor<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
    public T4 Value4 { get; }
    public T5 Value5 { get; }
    public T6 Value6 { get; }
    public T7 Value7 { get; }
    public T8 Value8 { get; }
    public T9 Value9 { get; }
    public T10 Value10 { get; }
}

file sealed class StateAccessor<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
    public T4 Value4 { get; }
    public T5 Value5 { get; }
    public T6 Value6 { get; }
    public T7 Value7 { get; }
    public T8 Value8 { get; }
    public T9 Value9 { get; }
    public T10 Value10 { get; }
    public T11 Value11 { get; }
}

file sealed class StateAccessor<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> {
    public T1 Value1 { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }
    public T4 Value4 { get; }
    public T5 Value5 { get; }
    public T6 Value6 { get; }
    public T7 Value7 { get; }
    public T8 Value8 { get; }
    public T9 Value9 { get; }
    public T10 Value10 { get; }
    public T11 Value11 { get; }
    public T12 Value12 { get; }
}