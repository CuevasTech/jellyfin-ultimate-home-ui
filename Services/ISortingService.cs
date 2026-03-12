using System.Collections.Generic;
using Jellyfin.Plugin.UltimateHomeUI.Models;

namespace Jellyfin.Plugin.UltimateHomeUI.Services;

/// <summary>
/// Contrato para el servicio de ordenación de secciones con resolución de empates.
/// </summary>
public interface ISortingService
{
    /// <summary>
    /// Ordena las secciones por peso y aplica la estrategia de empates.
    /// </summary>
    List<HomeSectionDto> SortSections(List<HomeSectionDto> sections, TieBreakingStrategy strategy);
}
