namespace Mumei.CodeGen.Components;

public abstract class SyntheticClassDefinition<TSelf> : SyntheticDeclarationDefinition, ISyntheticClass<TSelf> where TSelf : new() {
    public SyntheticIdentifier Name { get; private set; }

    // Add an analyzer that ensures Synthetic Classes are never instantiated by user code!
    protected SyntheticClassDefinition() {
        Name = typeof(TSelf).Name;
        // throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public virtual void Setup(ISyntheticClassBuilder<TSelf> classBuilder) { }

    public virtual void InternalBindCompilerOutputMembers(ISyntheticClassBuilder<TSelf> classBuilder) { }

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