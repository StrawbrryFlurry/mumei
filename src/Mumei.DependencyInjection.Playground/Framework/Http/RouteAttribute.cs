namespace Mumei.DependencyInjection.Playground.Framework.Http;

public sealed class RouteAttribute : Attribute {
  public RouteAttribute(string path) {
    Path = path;
  }

  public string Path { get; }
}