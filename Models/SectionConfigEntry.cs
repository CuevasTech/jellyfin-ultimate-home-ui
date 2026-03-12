namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Configuración persistible de una sección de la pantalla de inicio.
/// </summary>
public class SectionConfigEntry
{
    /// <summary>Gets or sets the unique section identifier.</summary>
    public string SectionId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the ordering weight (lower = first).</summary>
    public int Weight { get; set; }

    /// <summary>Gets or sets a value indicating whether this section is visible.</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>Gets or sets the card view type (0=Square, 1=Thumb, 2=Portrait, 3=Landscape).</summary>
    public int CardType { get; set; } = 2;

    /// <summary>Gets or sets the minimum items required to display the section.</summary>
    public int MinItems { get; set; }

    /// <summary>Gets or sets the maximum items to show in the carousel.</summary>
    public int MaxItems { get; set; } = 25;

    /// <summary>
    /// Gets or sets per-section hide-watched override.
    /// 0 = inherit from global, 1 = always show, 2 = always hide.
    /// </summary>
    public int HideWatchedOverride { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a built-in section.</summary>
    public bool IsBuiltIn { get; set; } = true;

    /// <summary>Gets or sets comma-separated media types for custom queries.</summary>
    public string MediaTypes { get; set; } = "Movie";

    /// <summary>Gets or sets comma-separated genre filters for custom queries.</summary>
    public string Genres { get; set; } = string.Empty;

    /// <summary>Gets or sets comma-separated studio filters for custom queries.</summary>
    public string Studios { get; set; } = string.Empty;

    /// <summary>Gets or sets comma-separated tag filters for custom queries.</summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>Gets or sets the minimum year for custom queries.</summary>
    public int MinYear { get; set; }

    /// <summary>Gets or sets the maximum year for custom queries.</summary>
    public int MaxYear { get; set; }

    /// <summary>Gets or sets the minimum community rating for custom queries.</summary>
    public double MinRating { get; set; }

    /// <summary>Gets or sets the sort field for custom queries.</summary>
    public string SortBy { get; set; } = "DateCreated";

    /// <summary>Gets or sets the sort direction (0=Ascending, 1=Descending).</summary>
    public int SortDirection { get; set; } = 1;
}
