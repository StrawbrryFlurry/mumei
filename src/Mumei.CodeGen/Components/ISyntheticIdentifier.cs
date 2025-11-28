namespace Mumei.CodeGen.Components;

public interface ISyntheticIdentifier {
    public string Resolve(IIdentifierResolver resolver);
}