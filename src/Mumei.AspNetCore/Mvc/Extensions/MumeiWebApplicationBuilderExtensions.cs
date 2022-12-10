using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mumei.AspNetCore.Common.Application;

namespace Mumei.AspNetCore.Mvc;

public static class MumeiWebApplicationBuilderExtensions {
  private const string ControllerPropertyActivatorInterfaceTypeAssemblyQualifiedName =
    "Microsoft.AspNetCore.Mvc.Controllers.IControllerPropertyActivator, Microsoft.AspNetCore.Mvc.Core";

  public static IMumeiWebApplicationBuilder AddControllers(
    this IMumeiWebApplicationBuilder builder
  ) {
    RemoveAspNetMvcServices(builder.Services);
    return builder;
  }

  private static void RemoveAspNetMvcServices(IServiceCollection services) {
    services.RemoveAll<IControllerFactory>();
    services.RemoveAll<IControllerActivator>();

    services.RemoveAll<IControllerFactoryProvider>();
    services.RemoveAll<IControllerActivatorProvider>();
    var controllerPropertyActivator = Type.GetType(ControllerPropertyActivatorInterfaceTypeAssemblyQualifiedName)!;
    services.RemoveAll(controllerPropertyActivator);

    services.RemoveAll<IActionInvokerFactory>();
    services.RemoveAll<IActionInvokerProvider>();
  }
}