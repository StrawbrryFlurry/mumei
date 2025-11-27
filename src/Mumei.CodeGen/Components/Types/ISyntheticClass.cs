using Mumei.CodeGen.Components.Types.Members;

namespace Mumei.CodeGen.Components.Types;

public interface ISyntheticClass<TClass> : ISyntheticClass { }

public interface ISyntheticClass : ISyntheticType, ISyntheticMember {
    public ref SyntheticFieldRef<T> Field<T>(string name);
    public ref SyntheticFieldRef<CompileTimeUnknown> Field(string name);

    public SyntheticMethodRef<Delegate> Method(string name);
    public SyntheticMethodRef<TSignature> Method<TSignature>(string name) where TSignature : Delegate;
}