using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public abstract class SyntheticClassDefinition<TSelf> : ISyntheticClass<TSelf> where TSelf : new() {
    public string Name { get; private set; }

    // Add an analyzer that ensures Synthetic Classes are never instantiated by user code!
    protected SyntheticClassDefinition() {
        Name = typeof(TSelf).Name;
        // throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public virtual void SetupDynamic(ISyntheticClassBuilder<TSelf> classBuilder) { }

    public IEnumerable<T> CompileTimeForEach<T>(IEnumerable<T> items) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public virtual void BindCompilerOutputMembers(ISyntheticClassBuilder<TSelf> classBuilder, TSelf target) { }

    public ref SyntheticFieldRef<T> Field<T>(string name) {
        throw new NotImplementedException();
    }

    public SyntheticNewExpression<TSelf> DynamicNew(object[] args) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ref SyntheticFieldRef<CompileTimeUnknown> Field(string name) {
        throw new NotImplementedException();
    }

    public SyntheticMethodRef<Delegate> Method(string name) {
        throw new NotImplementedException();
    }

    public SyntheticMethodRef<TSignature> Method<TSignature>(string name) where TSignature : Delegate {
        throw new NotImplementedException();
    }
}