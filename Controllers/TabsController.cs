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
/// CRUD para las pestañas de navegación superior.
/// </summary>
[ApiController]
[Route("Plugins/UltimateHomeUI")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class TabsController : ControllerBase
{
    private readonly IUserPreferencesService _preferencesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TabsController"/> class.
    /// </summary>
    public TabsController(IUserPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Obtiene las pestañas de navegación de un usuario.
    /// </summary>
    [HttpGet("Users/{userId}/Tabs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HomeTabDto>>> GetTabs([FromRoute] Guid userId)
    {
        var prefs = await _preferencesService.GetPreferencesAsync(userId).ConfigureAwait(false);
        return Ok(prefs?.Tabs ?? GetDefaultTabs());
    }

    /// <summary>
    /// Actualiza las pestañas de navegación de un usuario.
    /// </summary>
    [HttpPut("Users/{userId}/Tabs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HomeTabDto>>> UpdateTabs(
        [FromRoute] Guid userId,
        [FromBody] List<HomeTabDto> tabs)
    {
        ArgumentNullException.ThrowIfNull(tabs);

        var prefs = await _preferencesService.GetPreferencesAsync(userId).ConfigureAwait(false)
            ?? new UserPreferencesDto { UserId = userId };

        prefs.Tabs = tabs;
        await _preferencesService.UpdatePreferencesAsync(userId, prefs).ConfigureAwait(false);

        return Ok(tabs);
    }

    private static List<HomeTabDto> GetDefaultTabs()
    {
        return
        [
            new HomeTabDto { TabId = "home", Label = "Inicio", LinkType = TabLinkType.Home, Order = 0 },
            new HomeTabDto { TabId = "movies", Label = "Películas", LinkType = TabLinkType.Library, Order = 1 },
            new HomeTabDto { TabId = "series", Label = "Series", LinkType = TabLinkType.Library, Order = 2 },
            new HomeTabDto { TabId = "mylist", Label = "Mi Lista", LinkType = TabLinkType.MyList, Order = 3 },
        ];
    }
}
