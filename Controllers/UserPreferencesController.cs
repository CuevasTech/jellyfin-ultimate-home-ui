using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Jellyfin.Plugin.UltimateHomeUI.Models;
using Jellyfin.Plugin.UltimateHomeUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.UltimateHomeUI.Controllers;

/// <summary>
/// Endpoints REST para gestionar las preferencias de usuario.
/// </summary>
[ApiController]
[Route("Plugins/UltimateHomeUI")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class UserPreferencesController : ControllerBase
{
    private readonly IUserPreferencesService _preferencesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPreferencesController"/> class.
    /// </summary>
    public UserPreferencesController(IUserPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Obtiene las preferencias de pantalla de inicio de un usuario.
    /// </summary>
    [HttpGet("Users/{userId}/Preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences([FromRoute] Guid userId)
    {
        var preferences = await _preferencesService.GetPreferencesAsync(userId).ConfigureAwait(false);
        if (preferences is null)
        {
            return NotFound();
        }

        return Ok(preferences);
    }

    /// <summary>
    /// Actualiza las preferencias de pantalla de inicio de un usuario.
    /// </summary>
    [HttpPut("Users/{userId}/Preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserPreferencesDto>> UpdateUserPreferences(
        [FromRoute] Guid userId,
        [FromBody] UserPreferencesDto preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        if (preferences.UserId != userId)
        {
            return BadRequest("El userId del cuerpo no coincide con el de la ruta.");
        }

        var updated = await _preferencesService.UpdatePreferencesAsync(userId, preferences).ConfigureAwait(false);
        return Ok(updated);
    }

    /// <summary>
    /// Restablece las preferencias de un usuario a los valores por defecto.
    /// </summary>
    [HttpDelete("Users/{userId}/Preferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetUserPreferences([FromRoute] Guid userId)
    {
        await _preferencesService.ResetPreferencesAsync(userId).ConfigureAwait(false);
        return NoContent();
    }
}
