using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace WeatherApplication.Common;

[GlobalModule]
public partial class CommonModule {
  private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

  [Scoped]
  public abstract HttpClient HttpClient { get; }

  [Scoped]
  [Provides]
  public ILogger<TCategory> CreateLogger<TCategory>() {
    return _loggerFactory.CreateLogger<TCategory>();
  }
}

public abstract partial class CommonModule : IGlobalModule {
  public abstract IInjector Parent { get; }

  public abstract TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None);
  public abstract object Get(object token, InjectFlags flags = InjectFlags.None);
}