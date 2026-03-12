using System;

namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Elemento del Hero Slider con metadatos enriquecidos para renderizado.
/// </summary>
public class HeroItemDto
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
    /// Gets or sets the tagline.
    /// </summary>
    public string? Tagline { get; set; }

    /// <summary>
    /// Gets or sets a short overview (truncated to ~200 chars).
    /// </summary>
    public string? Overview { get; set; }

    /// <summary>
    /// Gets or sets the production year.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Gets or sets the official content rating (PG-13, R, etc.).
    /// </summary>
    public string? OfficialRating { get; set; }

    /// <summary>
    /// Gets or sets the community rating.
    /// </summary>
    public double? CommunityRating { get; set; }

    /// <summary>
    /// Gets or sets the runtime in minutes.
    /// </summary>
    public int? RuntimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the genres as a comma-separated string.
    /// </summary>
    public string? Genres { get; set; }

    /// <summary>
    /// Gets or sets the relative URL to the backdrop image.
    /// </summary>
    public string? BackdropUrl { get; set; }

    /// <summary>
    /// Gets or sets the relative URL to the logo image.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Gets or sets the relative URL to the trailer video (for background autoplay).
    /// </summary>
    public string? TrailerUrl { get; set; }

    /// <summary>
    /// Gets or sets the media type (Movie, Series, etc.).
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is a favorite.
    /// </summary>
    public bool IsFavorite { get; set; }
}
