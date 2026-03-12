namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Estrategia para resolver empates entre secciones con el mismo peso numérico.
/// </summary>
public enum TieBreakingStrategy
{
    /// <summary>Orden aleatorio en cada recarga.</summary>
    Random,

    /// <summary>Orden alfabético por nombre de sección.</summary>
    Alphabetical,

    /// <summary>Prioridad por actividad más reciente del usuario.</summary>
    LastActivity,

    /// <summary>Orden original del servidor.</summary>
    ServerDefault,
}
