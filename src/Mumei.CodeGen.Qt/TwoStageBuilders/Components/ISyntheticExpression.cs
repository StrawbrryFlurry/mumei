namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticExpression { }
public interface ILiteralExpression : ISyntheticExpression { }
public interface ISyntheticStringLiteralExpression : ILiteralExpression { }

public sealed class SyntheticStringLiteralExpression : ILiteralExpression {
    public static implicit operator string(SyntheticStringLiteralExpression expr) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public sealed class SyntheticRawStringLiteralExpression(string value, string quotes) : ILiteralExpression {
    public static implicit operator string(SyntheticRawStringLiteralExpression expr) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}