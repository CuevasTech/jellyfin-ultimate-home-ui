using System;

namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Tarjeta de media genérica para cualquier tipo de vista en un carrusel.
/// </summary>
public class MediaCardDto
{
    /// <summary>
    /// Gets or sets the Jellyfin item ID.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subtitle (e.g. season/episode info, year).
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets the relative URL to the primary image.
    /// </summary>
    public string? PrimaryImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the relative URL to the backdrop/thumb image.
    /// </summary>
    public string? BackdropImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the media type (Movie, Series, Episode, etc.).
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// Gets or sets the production year.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Gets or sets the community rating.
    /// </summary>
    public double? CommunityRating { get; set; }

    /// <summary>
    /// Gets or sets the official content rating.
    /// </summary>
    public string? OfficialRating { get; set; }

    /// <summary>
    /// Gets or sets the playback progress as a percentage (0.0 to 1.0).
    /// </summary>
    public double? PlaybackProgress { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item has been played.
    /// </summary>
    public bool IsPlayed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is a favorite.
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// Gets or sets the unplayed episode count (for Series).
    /// </summary>
    public int? UnplayedCount { get; set; }
}
