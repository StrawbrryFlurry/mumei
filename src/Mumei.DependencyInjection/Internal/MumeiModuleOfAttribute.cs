using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.Internal;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MumeiModuleOfAttribute<TModule> : Attribute where TModule : IModule { }