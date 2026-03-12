using System;
using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Representa una pestaña de la navegación superior.
/// </summary>
public class HomeTabDto
{
    /// <summary>
    /// Gets or sets the unique tab identifier.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TabId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display label of the tab.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tab link type.
    /// </summary>
    public TabLinkType LinkType { get; set; } = TabLinkType.Home;

    /// <summary>
    /// Gets or sets the target ID (collection, genre, playlist ID).
    /// </summary>
    public Guid? TargetId { get; set; }

    /// <summary>
    /// Gets or sets a custom genre name when LinkType is Genre.
    /// </summary>
    [StringLength(100)]
    public string? GenreName { get; set; }

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    [Range(0, 100)]
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this tab is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;
}
