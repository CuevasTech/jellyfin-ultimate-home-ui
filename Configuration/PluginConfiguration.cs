using System.Collections.Generic;
using Jellyfin.Plugin.UltimateHomeUI.Models;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.UltimateHomeUI.Configuration;

/// <summary>
/// Configuración global del plugin (compartida por todos los usuarios).
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    /// <remarks>
    /// IMPORTANTE: NO pre-poblar Sections ni Tabs aquí.
    /// El XmlSerializer de Jellyfin llama al constructor y LUEGO añade los items
    /// del XML encima de los ya existentes. Si el constructor pre-puebla la lista,
    /// cada reinicio del servidor la duplica (7→14→21→...).
    /// Los defaults se inicializan una sola vez en Plugin.cs, solo cuando la lista está vacía.
    /// </remarks>
    public PluginConfiguration()
    {
        EnableCustomLayout = true;
        DefaultTieBreakingStrategy = TieBreakingStrategy.Random;
        MaxSectionsVisible = 15;
        HeroSliderEnabled = true;
        HeroSliderSource = HeroSource.LatestAdded;
        HeroSliderMaxItems = 5;
        HeroSliderAutoPlayTrailer = true;
        HeroSliderIntervalSeconds = 8;
        HeroSliderCollectionId = string.Empty;
        HeroSliderGenreName = string.Empty;
        HideWatchedGlobally = false;
        Sections = [];
        Tabs = [];
    }

    // ── General ─────────────────────────────────────────────

    /// <summary>Gets or sets a value indicating whether custom layout is enabled globally.</summary>
    public bool EnableCustomLayout { get; set; }

    /// <summary>Gets or sets the default tie-breaking strategy.</summary>
    public TieBreakingStrategy DefaultTieBreakingStrategy { get; set; }

    /// <summary>Gets or sets the maximum number of sections visible on the home screen.</summary>
    public int MaxSectionsVisible { get; set; }

    /// <summary>Gets or sets a value indicating whether watched content is hidden globally.</summary>
    public bool HideWatchedGlobally { get; set; }

    // ── Hero Slider ─────────────────────────────────────────

    /// <summary>Gets or sets a value indicating whether the hero slider is enabled.</summary>
    public bool HeroSliderEnabled { get; set; }

    /// <summary>Gets or sets the content source for the hero slider.</summary>
    public HeroSource HeroSliderSource { get; set; }

    /// <summary>Gets or sets the maximum number of items in the hero slider.</summary>
    public int HeroSliderMaxItems { get; set; }

    /// <summary>Gets or sets a value indicating whether trailers autoplay in the hero.</summary>
    public bool HeroSliderAutoPlayTrailer { get; set; }

    /// <summary>Gets or sets the hero rotation interval in seconds.</summary>
    public int HeroSliderIntervalSeconds { get; set; }

    /// <summary>Gets or sets the collection ID when hero source is Collection.</summary>
    public string HeroSliderCollectionId { get; set; }

    /// <summary>Gets or sets the genre name when hero source is Genre.</summary>
    public string HeroSliderGenreName { get; set; }

    // ── Sections ────────────────────────────────────────────

    /// <summary>Gets or sets the full list of section configurations.</summary>
    public List<SectionConfigEntry> Sections { get; set; }

    // ── Tabs ────────────────────────────────────────────────

    /// <summary>Gets or sets the top navigation tabs.</summary>
    public List<TabConfigEntry> Tabs { get; set; }

    // ── Defaults ────────────────────────────────────────────

    /// <summary>Returns the factory-default section list.</summary>
    public static List<SectionConfigEntry> GetDefaultSections() =>
    [
        new() { SectionId = "ContinueWatching", DisplayName = "Continuar viendo", Weight = 1, IsBuiltIn = true, CardType = 3 },
        new() { SectionId = "LatestAdded", DisplayName = "Últimas añadidas", Weight = 2, IsBuiltIn = true, CardType = 2 },
        new() { SectionId = "Favorites", DisplayName = "Mis favoritos", Weight = 3, IsBuiltIn = true, CardType = 2 },
        new() { SectionId = "BecauseYouWatched", DisplayName = "Porque viste...", Weight = 4, IsBuiltIn = true, CardType = 2 },
        new() { SectionId = "WatchAgain", DisplayName = "Ver otra vez", Weight = 5, IsBuiltIn = true, CardType = 2 },
        new() { SectionId = "HiddenGems", DisplayName = "Joyas ocultas", Weight = 6, IsBuiltIn = true, CardType = 2 },
        new() { SectionId = "TimeSlotSuggestions", DisplayName = "Sugerencias por horario", Weight = 7, IsBuiltIn = true, CardType = 1 },
    ];

    /// <summary>Returns the factory-default tab list.</summary>
    public static List<TabConfigEntry> GetDefaultTabs() =>
    [
        new() { TabId = "home", Label = "Inicio", LinkType = 0, Order = 0 },
        new() { TabId = "favorites", Label = "Mis favoritos", LinkType = 4, Order = 1 },
    ];
}
