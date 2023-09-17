using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;

namespace WeatherApplication.Common;

[GlobalModule]
public partial class CommonModule {
  private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

  [Scoped]
  public abstract HttpClient HttpClient { get; }

  [ProvideScoped]
  public ILogger<TCategory> CreateLogger<TCategory>() {
    return _loggerFactory.CreateLogger<TCategory>();
  }
}

public abstract partial class CommonModule : IGlobalModule {
  public abstract IInjector Parent { get; }

  public abstract TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None);
  public abstract object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None);
}