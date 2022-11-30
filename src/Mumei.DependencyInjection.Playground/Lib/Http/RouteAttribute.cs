namespace Mumei.DependencyInjection.Playground.Lib.Http; 

public class RouteAttribute : Attribute {
  public string Path { get; }

  public RouteAttribute(string path) {
    Path = path;
  }
}