using Mumei.Core;
using Mumei.DependencyInjection.Playground.Common;
using HttpClient = Mumei.DependencyInjection.Playground.Common.HttpClient;

namespace Mumei.DependencyInjection.Playground.Weather.generated;

public class HttpClientλFactory : Provider<IHttpClient> {
  public override IHttpClient Get() {
    return new HttpClient();
  }
}