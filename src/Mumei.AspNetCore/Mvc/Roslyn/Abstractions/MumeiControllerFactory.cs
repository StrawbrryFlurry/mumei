using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Mumei.AspNetCore.Mvc.Roslyn;

public abstract class MumeiControllerFactory : IControllerFactory {
  public object CreateController(ControllerContext context) {
    throw new NotImplementedException();
  }

  public void ReleaseController(ControllerContext context, object controller) {
    throw new NotImplementedException();
  }
}