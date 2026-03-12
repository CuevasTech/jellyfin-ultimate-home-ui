namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Fuente de contenido para el Hero Slider de la cabecera.
/// </summary>
public enum HeroSource
{
    /// <summary>Últimos elementos añadidos a la biblioteca.</summary>
    LatestAdded,

    /// <summary>Selección aleatoria de la biblioteca.</summary>
    Random,

    /// <summary>Contenido de una colección (BoxSet) específica.</summary>
    Collection,

    /// <summary>Contenido filtrado por género.</summary>
    Genre,

    /// <summary>Contenido en tendencia (más visto recientemente).</summary>
    Trending,
}
