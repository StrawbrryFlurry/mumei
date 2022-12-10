using Microsoft.AspNetCore.Mvc;
using Mumei.AspNetCore.Common.Mvc;
using Mumei.AspNetCore.Example.Cats.Services;

namespace Mumei.AspNetCore.Example.Cats;

[Route("api/[controller]")]
public sealed class CatsController : MumeiControllerBase {
  private readonly ICatService _catService;

  public CatsController(ICatService catService) {
    _catService = catService;
  }

  [HttpGet]
  public async Task<IActionResult> GetCats() {
    var cats = await _catService.GetCatsAsync();
    return Ok(cats);
  }
}