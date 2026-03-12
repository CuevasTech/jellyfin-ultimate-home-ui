using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Representa una sección completa de la pantalla de inicio con su configuración de vista y datos.
/// </summary>
public class HomeSectionDto
{
    /// <summary>
    /// Gets or sets the unique section identifier.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SectionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display title of the section.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the card view type for this section.
    /// </summary>
    public CardViewType CardType { get; set; } = CardViewType.Portrait;

    /// <summary>
    /// Gets or sets the minimum items required to show this section.
    /// </summary>
    [Range(0, 100)]
    public int MinItems { get; set; }

    /// <summary>
    /// Gets or sets the maximum items to display in the carousel.
    /// </summary>
    [Range(1, 100)]
    public int MaxItems { get; set; } = 25;

    /// <summary>
    /// Gets or sets the numeric weight for ordering (lower = first).
    /// </summary>
    [Range(0, 1000)]
    public int Weight { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this section is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether watched items are hidden in this section.
    /// Null inherits from global setting.
    /// </summary>
    public bool? HideWatched { get; set; }

    /// <summary>
    /// Gets or sets the query filter for custom sections.
    /// </summary>
    public SectionQueryDto? Query { get; set; }

    /// <summary>
    /// Gets or sets the built-in section type. Null for custom query sections.
    /// </summary>
    public BuiltInSection? BuiltInType { get; set; }

    /// <summary>
    /// Gets or sets the media cards loaded for this section.
    /// </summary>
    public List<MediaCardDto> Items { get; set; } = [];
}
