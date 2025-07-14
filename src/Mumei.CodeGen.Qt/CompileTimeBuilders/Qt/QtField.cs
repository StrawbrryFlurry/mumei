using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public sealed class QtField<T> : IQtCompileTimeValue<T>, IQtTemplateBindable, IQtComponent {
    private readonly AccessModifier _modifiers;
    private readonly string _name;

    private IQtType _type;

    public QtField(
        AccessModifier modifiers,
        string name
    ) {
        _modifiers = modifiers;
        _name = name;
        _type = QtType.ForRuntimeType<T>();
    }

    public QtField(
        AccessModifier modifiers,
        string name,
        IQtType type
    ) {
        _modifiers = modifiers;
        _name = name;
        _type = type;
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

    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteFormatted(
            $$"""
              {{_modifiers}} {{_type:g}} {{_name}};
              """
        );
    }
}