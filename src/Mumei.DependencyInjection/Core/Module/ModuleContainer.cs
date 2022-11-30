using Mumei.Attributes;

namespace Mumei.Core; 

[Injectable(providedIn: ProvidedIn.Root)]
public class ModuleContainer : List<IModuleRef> {
}