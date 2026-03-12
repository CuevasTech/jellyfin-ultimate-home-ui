using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Filtros complejos para definir el contenido de una sección personalizada.
/// </summary>
public class SectionQueryDto
{
    /// <summary>
    /// Gets or sets the media types to include (Movie, Series, Episode, etc.).
    /// </summary>
    public List<string> MediaTypes { get; set; } = ["Movie"];

    /// <summary>
    /// Gets or sets genre filters (OR logic within).
    /// </summary>
    public List<string> Genres { get; set; } = [];

    /// <summary>
    /// Gets or sets studio filters (OR logic within).
    /// </summary>
    public List<string> Studios { get; set; } = [];

    /// <summary>
    /// Gets or sets tag filters.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the minimum production year (inclusive).
    /// </summary>
    public int? MinYear { get; set; }

    /// <summary>
    /// Gets or sets the maximum production year (inclusive).
    /// </summary>
    public int? MaxYear { get; set; }

    /// <summary>
    /// Gets or sets the minimum community rating (inclusive).
    /// </summary>
    public double? MinRating { get; set; }

    /// <summary>
    /// Gets or sets the maximum community rating (inclusive).
    /// </summary>
    public double? MaxRating { get; set; }

    /// <summary>
    /// Gets or sets the parent collection ID (BoxSet).
    /// </summary>
    public Guid? CollectionId { get; set; }

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    public string SortBy { get; set; } = "DateCreated";

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
}
