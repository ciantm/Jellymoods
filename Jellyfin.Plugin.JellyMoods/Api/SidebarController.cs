using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.JellyMoods.Api;

/// <summary>
/// Serves static JS assets for JellyMoods so they can be loaded globally
/// by Jellyfin's web client via config.json.
/// </summary>
[ApiController]
[Route("JellyMoods")]
public class SidebarController : ControllerBase
{
    /// <summary>
    /// Serves the sidebar injection script.
    /// Accessible at: /JellyMoods/sidebar.js
    /// </summary>
    [HttpGet("sidebar.js")]
    [Produces("application/javascript")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetSidebarScript()
    {
        var resourceName = $"{GetType().Namespace?.Replace(".Api", "")}.Configuration.sidebar.js";
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return NotFound();
        }

        using var reader = new StreamReader(stream);
        var js = reader.ReadToEnd();
        return Content(js, "application/javascript");
    }
}
