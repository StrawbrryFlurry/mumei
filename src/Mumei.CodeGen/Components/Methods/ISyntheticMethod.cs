namespace Mumei.CodeGen.Components.Methods;

public interface ISyntheticMethod<TSignature> : ISyntheticMethod where TSignature : Delegate {
    public TSignature Bind(object target);
}

public interface ISyntheticMethod {
    public string Name { get; }

    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate;
}