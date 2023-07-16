using Mumei.DependencyInjection.Providers.Dynamic.Registration;

namespace Mumei.DependencyInjection.Module.Registration;

public sealed class DynamicallyBindAttribute<TBinder> : Attribute where TBinder : IProviderBinder { }