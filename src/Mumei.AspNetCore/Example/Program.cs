using Mumei.AspNetCore.Application;
using Mumei.AspNetCore.Mvc;

namespace Mumei.AspNetCore.Example;

public class Program {
  public static async Task Main() {
    var web = WebApplication.CreateBuilder();
    web.Services.AddControllers();
    var builder = MumeiWebApplication.CreateBuilder<IAppModule>();

    builder.AddControllers();

    var app = builder.Build();

    app.MapControllers();

    await app.RunAsync();
  }
}