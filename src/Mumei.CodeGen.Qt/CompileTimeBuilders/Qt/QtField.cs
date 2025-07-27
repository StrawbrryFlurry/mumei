using Mumei.CodeGen.Qt;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public readonly struct QtField<T> : IQtCompileTimeValue, IQtTemplateBindable {
    internal QtFieldCore Field { get; }

    public QtField(
        AccessModifier modifiers,
        string name
    ) {
        Field = new QtFieldCore(
            modifiers,
            name,
            typeof(T)
        );
    }

    public T Get(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TDynamic DynamicGet<TDynamic>(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void Set(IQtThis target, T value) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void DynamicSet<TDynamic>(IQtThis target, TDynamic value) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T1 As<T1>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}

internal readonly struct QtFieldCore : IQtTemplateBindable {
    private readonly AccessModifier _modifiers;
    private readonly string _name;

    private readonly IQtType _type;

    public QtFieldCore(
        AccessModifier modifiers,
        string name,
        Type type
    ) {
        _modifiers = modifiers;
        _name = name;
        _type = QtType.ForRuntimeType(type);
    }

    public QtFieldCore(
        AccessModifier modifiers,
        string name,
        IQtType type
    ) {
        _modifiers = modifiers;
        _name = name;
        _type = type;
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteFormatted(
            $$"""
              {{_modifiers}} {{_type:g}} {{_name}};
              """
        );
    }
}