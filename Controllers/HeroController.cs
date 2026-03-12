using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Jellyfin.Plugin.UltimateHomeUI.Models;
using Jellyfin.Plugin.UltimateHomeUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.UltimateHomeUI.Controllers;

/// <summary>
/// Endpoint para obtener el contenido del Hero Slider.
/// </summary>
[ApiController]
[Route("Plugins/UltimateHomeUI")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class HeroController : ControllerBase
{
    private readonly IHeroService _heroService;
    private readonly IUserPreferencesService _preferencesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroController"/> class.
    /// </summary>
    public HeroController(IHeroService heroService, IUserPreferencesService preferencesService)
    {
        _heroService = heroService;
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Obtiene los elementos del Hero Slider para un usuario.
    /// </summary>
    [HttpGet("Users/{userId}/Hero")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HeroItemDto>>> GetHeroItems(
        [FromRoute] Guid userId,
        [FromQuery] HeroSource? source = null,
        [FromQuery] int? maxItems = null)
    {
        var config = Plugin.Instance?.Configuration;
        var prefs = await _preferencesService.GetPreferencesAsync(userId).ConfigureAwait(false);

        var effectiveSource = source
            ?? prefs?.HeroSource
            ?? config?.HeroSliderSource
            ?? HeroSource.LatestAdded;

        var effectiveMax = maxItems
            ?? config?.HeroSliderMaxItems
            ?? 5;

        var items = await _heroService.GetHeroItemsAsync(userId, effectiveSource, effectiveMax).ConfigureAwait(false);
        return Ok(items);
    }
}
