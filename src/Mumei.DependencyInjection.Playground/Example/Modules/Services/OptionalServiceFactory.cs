using Mumei.Core;

namespace Mumei.DependencyInjection.Playground.Example.Modules.Services;

public class OptionalServiceλFactory : IProviderFactory<OptionalService> {
  public OptionalService Get() {
    return null!;
  }
}