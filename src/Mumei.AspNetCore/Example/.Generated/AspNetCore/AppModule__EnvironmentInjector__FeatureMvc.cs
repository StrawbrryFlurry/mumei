using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Mumei.AspNetCore.Mvc;

namespace Mumei.AspNetCore.Example.Generated;

internal sealed partial class AppModuleλEnvironmentInjector : IMvcEnvironmentInjector {
  public IControllerFactory ControllerFactory { get; }
  public IControllerActivator ControllerActivator { get; }
  public IControllerFactoryProvider ControllerActionInvokerFactory { get; }
  public IControllerActivatorProvider ControllerActionArgumentBinder { get; }
  public object ControllerPropertyActivator { get; }
  public IActionInvokerFactory ActionInvokerFactory { get; }
  public IActionInvokerProvider ActionInvokerProvider { get; }
}