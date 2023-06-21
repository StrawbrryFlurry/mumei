using Mumei.DependencyInjection.Attributes;

namespace Mumei.DependencyInjection.Core;

[Injectable(ProvidedIn.Root)]
public class ModuleContainer : List<IModuleRef<IModule>> { }