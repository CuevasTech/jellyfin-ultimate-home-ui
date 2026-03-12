namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Identificadores de secciones inteligentes preestablecidas.
/// </summary>
public enum BuiltInSection
{
    /// <summary>Últimos elementos añadidos.</summary>
    LatestAdded,

    /// <summary>Continuar viendo (con progreso).</summary>
    ContinueWatching,

    /// <summary>Mis favoritos.</summary>
    Favorites,

    /// <summary>Porque viste [última película vista].</summary>
    BecauseYouWatched,

    /// <summary>Ver otra vez (bien puntuadas, vistas hace tiempo).</summary>
    WatchAgain,

    /// <summary>Joyas ocultas (alto rating, poco vistas).</summary>
    HiddenGems,

    /// <summary>Sugerencias por franja horaria.</summary>
    TimeSlotSuggestions,
}
