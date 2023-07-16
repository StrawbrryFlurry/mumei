using Mumei.DependencyInjection.Injector;

namespace Mumei.DependencyInjection.Providers.Dynamic;

public delegate object DynamicProviderFactory(IInjector injector, IInjector? scope = null);