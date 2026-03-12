using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.UltimateHomeUI.Models;

namespace Jellyfin.Plugin.UltimateHomeUI.Services;

/// <summary>
/// Contrato para el servicio de consultas dinámicas de secciones.
/// </summary>
public interface ISectionQueryService
{
    /// <summary>
    /// Ejecuta la consulta definida en una sección y devuelve las tarjetas de media.
    /// </summary>
    Task<List<MediaCardDto>> ExecuteSectionQueryAsync(Guid userId, HomeSectionDto section);

    /// <summary>
    /// Obtiene las secciones inteligentes preestablecidas con sus datos.
    /// </summary>
    Task<List<HomeSectionDto>> GetBuiltInSectionsAsync(Guid userId);
}
