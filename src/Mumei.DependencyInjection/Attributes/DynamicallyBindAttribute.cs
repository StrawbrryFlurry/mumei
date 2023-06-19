using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.Attributes;

public sealed class DynamicallyBindAttribute<TBinder> : Attribute where TBinder : IProviderBinder { }