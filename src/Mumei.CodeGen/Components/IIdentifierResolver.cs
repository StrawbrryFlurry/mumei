namespace Mumei.CodeGen.Components;

public interface IIdentifierResolver {
    public string MakeGloballyUnique(string name);
    public string MakeUnique(object scope, string name);
}