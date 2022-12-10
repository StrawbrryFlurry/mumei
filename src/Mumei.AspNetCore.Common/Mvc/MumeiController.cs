using Microsoft.AspNetCore.Mvc;
using Mumei.DependencyInjection.Attributes;

namespace Mumei.AspNetCore.Common.Mvc;

[Component]
public abstract class MumeiController : Controller { }