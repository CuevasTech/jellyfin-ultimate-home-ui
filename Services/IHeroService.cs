using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.UltimateHomeUI.Models;

namespace Jellyfin.Plugin.UltimateHomeUI.Services;

/// <summary>
/// Contrato para el servicio que proporciona contenido al Hero Slider.
/// </summary>
public interface IHeroService
{
    /// <summary>
    /// Obtiene los elementos para el Hero Slider según la configuración.
    /// </summary>
    Task<List<HeroItemDto>> GetHeroItemsAsync(Guid userId, HeroSource source, int maxItems);
}
