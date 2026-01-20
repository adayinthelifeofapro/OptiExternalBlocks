using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiExternalBlocks.Common;

namespace OptiExternalBlocks.Controllers;

[Authorize(Policy = OptiExternalBlocksConstants.AuthorizationPolicy)]
public sealed class AdministrationController : Controller
{
    [HttpGet]
    [Route("~/optimizely-externalblocks/administration/templates")]
    public IActionResult Templates()
    {
        return View("~/Views/Administration/Templates.cshtml");
    }

    [HttpGet]
    [Route("~/optimizely-externalblocks/administration/endpoints")]
    public IActionResult Endpoints()
    {
        return View("~/Views/Administration/Endpoints.cshtml");
    }

    [HttpGet]
    [Route("~/optimizely-externalblocks/administration/about")]
    public IActionResult About()
    {
        return View("~/Views/Administration/About.cshtml");
    }
}
