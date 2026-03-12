using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Representa el peso numérico asignado a una sección de la pantalla de inicio.
/// </summary>
public class SectionWeightDto
{
    /// <summary>
    /// Gets or sets the section identifier.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SectionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the section.
    /// </summary>
    [StringLength(200)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the numeric weight (lower values appear first).
    /// </summary>
    [Range(0, 1000)]
    public int Weight { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this section is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;
}
