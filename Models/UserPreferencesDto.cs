using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Preferencias de pantalla de inicio de un usuario concreto.
/// </summary>
public class UserPreferencesDto
{
    /// <summary>
    /// Gets or sets the user ID these preferences belong to.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the tie-breaking strategy for this user.
    /// Null means use the plugin's global default.
    /// </summary>
    public TieBreakingStrategy? TieBreakingStrategy { get; set; }

    /// <summary>
    /// Gets or sets the section weights ordered by priority.
    /// </summary>
    public List<SectionWeightDto> SectionWeights { get; set; } = [];

    /// <summary>
    /// Gets or sets the custom tabs for the top navigation.
    /// </summary>
    public List<HomeTabDto> Tabs { get; set; } = [];

    /// <summary>
    /// Gets or sets the hero slider source override. Null uses plugin default.
    /// </summary>
    public HeroSource? HeroSource { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether watched content should be hidden.
    /// Null uses plugin default.
    /// </summary>
    public bool? HideWatched { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has overridden global settings.
    /// </summary>
    public bool HasCustomLayout { get; set; }
}
