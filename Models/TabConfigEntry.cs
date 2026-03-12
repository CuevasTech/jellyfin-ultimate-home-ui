namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Configuración persistible de una pestaña de navegación superior.
/// </summary>
public class TabConfigEntry
{
    /// <summary>Gets or sets the unique tab identifier.</summary>
    public string TabId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display label.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the link type (0=Home, 1=Collection, 2=Genre, 3=Playlist, 4=MyList, 5=Library).</summary>
    public int LinkType { get; set; }

    /// <summary>Gets or sets the target ID (collection, playlist, or library).</summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>Gets or sets the genre name when LinkType is Genre.</summary>
    public string GenreName { get; set; } = string.Empty;

    /// <summary>Gets or sets the display order.</summary>
    public int Order { get; set; }

    /// <summary>Gets or sets a value indicating whether this tab is visible.</summary>
    public bool IsVisible { get; set; } = true;
}
