using Microsoft.Extensions.DependencyInjection;
using Mumei.Core.Attributes;

namespace Mumei.DependencyInjection.Playground;

public class PropertyInjection {
  [Inject]
  public IServiceScopeFactory ScopeFactory { get; set; }
}