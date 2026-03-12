namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Tipo de enlace para las pestañas de navegación superior.
/// </summary>
public enum TabLinkType
{
    /// <summary>Enlace a la pantalla de inicio.</summary>
    Home,

    /// <summary>Enlace a una colección (BoxSet).</summary>
    Collection,

    /// <summary>Enlace a un género.</summary>
    Genre,

    /// <summary>Enlace a una lista de reproducción.</summary>
    Playlist,

    /// <summary>Enlace a "Mi Lista" (favoritos).</summary>
    MyList,

    /// <summary>Enlace a una biblioteca específica.</summary>
    Library,
}
