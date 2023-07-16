namespace Mumei.DependencyInjection.Module.CodeGen;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MumeiModuleImplFor<TModule> : Attribute where TModule : IModule { }