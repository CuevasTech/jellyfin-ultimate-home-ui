using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.UltimateHomeUI.Controllers;

/// <summary>
/// Sirve los archivos JS/CSS del frontend embebidos como recursos del plugin.
/// </summary>
[ApiController]
[Route("UltimateHomeUI")]
[AllowAnonymous]
public class WebAssetsController : ControllerBase
{
    private static readonly Assembly PluginAssembly = typeof(Plugin).Assembly;

    /// <summary>Sirve un archivo JS o CSS del frontend.</summary>
    [HttpGet("Web/{filename}")]
    public ActionResult GetAsset([FromRoute] string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return NotFound();
        }

        var sanitized = filename.Replace("..", string.Empty, StringComparison.Ordinal);
        var resourceName = PluginAssembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("." + sanitized, StringComparison.OrdinalIgnoreCase)
                              || n.EndsWith("." + sanitized.Replace("-", "_", StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            return NotFound();
        }

        var stream = PluginAssembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return NotFound();
        }

        var contentType = sanitized.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
            ? "application/javascript"
            : sanitized.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                ? "text/css"
                : "application/octet-stream";

        return File(stream, contentType);
    }
}
