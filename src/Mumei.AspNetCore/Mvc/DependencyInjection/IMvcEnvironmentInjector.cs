using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Mumei.AspNetCore.Mvc;

internal interface IMvcEnvironmentInjector {
  public IControllerFactory ControllerFactory { get; }
  public IControllerActivator ControllerActivator { get; }

  public IControllerFactoryProvider ControllerActionInvokerFactory { get; }
  public IControllerActivatorProvider ControllerActionArgumentBinder { get; }

  /// <summary>
  ///   IControllerPropertyActivator
  /// </summary>
  public object ControllerPropertyActivator { get; }

  public IActionInvokerFactory ActionInvokerFactory { get; }
  public IActionInvokerProvider ActionInvokerProvider { get; }
}