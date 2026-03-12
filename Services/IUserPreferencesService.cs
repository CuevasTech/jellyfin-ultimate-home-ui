using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.UltimateHomeUI.Models;

namespace Jellyfin.Plugin.UltimateHomeUI.Services;

/// <summary>
/// Contrato para el servicio de gestión de preferencias de usuario.
/// </summary>
public interface IUserPreferencesService
{
    /// <summary>
    /// Obtiene las preferencias del usuario.
    /// </summary>
    Task<UserPreferencesDto?> GetPreferencesAsync(Guid userId);

    /// <summary>
    /// Actualiza las preferencias del usuario.
    /// </summary>
    Task<UserPreferencesDto> UpdatePreferencesAsync(Guid userId, UserPreferencesDto preferences);

    /// <summary>
    /// Elimina las preferencias del usuario, restaurando los valores por defecto.
    /// </summary>
    Task ResetPreferencesAsync(Guid userId);
}
