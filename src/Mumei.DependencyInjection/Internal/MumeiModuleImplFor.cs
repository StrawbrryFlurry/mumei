using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.Internal;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MumeiModuleImplFor<TModule> : Attribute where TModule : IModule { }