namespace Mumei.DependencyInjection.Core;

public delegate object ProviderFactory(IInjector injector, IInjector? scope = null);