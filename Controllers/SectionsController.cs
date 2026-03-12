using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Jellyfin.Plugin.UltimateHomeUI.Models;
using Jellyfin.Plugin.UltimateHomeUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.UltimateHomeUI.Controllers;

/// <summary>
/// CRUD para secciones personalizadas de la pantalla de inicio.
/// </summary>
[ApiController]
[Route("Plugins/UltimateHomeUI")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class SectionsController : ControllerBase
{
    private readonly IUserPreferencesService _preferencesService;
    private readonly ISectionQueryService _sectionQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SectionsController"/> class.
    /// </summary>
    public SectionsController(
        IUserPreferencesService preferencesService,
        ISectionQueryService sectionQueryService)
    {
        _preferencesService = preferencesService;
        _sectionQueryService = sectionQueryService;
    }

    /// <summary>
    /// Obtiene los pesos de secciones del usuario.
    /// </summary>
    [HttpGet("Users/{userId}/Sections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SectionWeightDto>>> GetSections([FromRoute] Guid userId)
    {
        var prefs = await _preferencesService.GetPreferencesAsync(userId).ConfigureAwait(false);
        return Ok(prefs?.SectionWeights ?? new List<SectionWeightDto>());
    }

    /// <summary>
    /// Actualiza los pesos de secciones del usuario.
    /// </summary>
    [HttpPut("Users/{userId}/Sections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SectionWeightDto>>> UpdateSections(
        [FromRoute] Guid userId,
        [FromBody] List<SectionWeightDto> sectionWeights)
    {
        ArgumentNullException.ThrowIfNull(sectionWeights);

        var prefs = await _preferencesService.GetPreferencesAsync(userId).ConfigureAwait(false)
            ?? new UserPreferencesDto { UserId = userId };

        prefs.SectionWeights = sectionWeights;
        await _preferencesService.UpdatePreferencesAsync(userId, prefs).ConfigureAwait(false);

        return Ok(sectionWeights);
    }

    /// <summary>
    /// Ejecuta una consulta de sección personalizada y devuelve las tarjetas de media.
    /// </summary>
    [HttpPost("Users/{userId}/Sections/Preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MediaCardDto>>> PreviewSection(
        [FromRoute] Guid userId,
        [FromBody] HomeSectionDto section)
    {
        ArgumentNullException.ThrowIfNull(section);

        var items = await _sectionQueryService.ExecuteSectionQueryAsync(userId, section).ConfigureAwait(false);
        return Ok(items);
    }
}
