using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Enums;
using Jellyfin.Plugin.UltimateHomeUI.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.UltimateHomeUI.Services;

/// <summary>
/// Ejecuta consultas dinámicas contra ILibraryManager para poblar las secciones del home.
/// </summary>
public class SectionQueryService : ISectionQueryService
{
    private readonly ILibraryManager _libraryManager;
    private readonly IUserManager _userManager;
    private readonly IUserDataManager _userDataManager;
    private readonly ILogger<SectionQueryService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SectionQueryService"/> class.
    /// </summary>
    public SectionQueryService(
        ILibraryManager libraryManager,
        IUserManager userManager,
        IUserDataManager userDataManager,
        ILogger<SectionQueryService> logger)
    {
        _libraryManager = libraryManager;
        _userManager = userManager;
        _userDataManager = userDataManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<List<MediaCardDto>> ExecuteSectionQueryAsync(Guid userId, HomeSectionDto section)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (section.BuiltInType.HasValue)
        {
            return ExecuteBuiltInQueryAsync(userId, section);
        }

        return ExecuteCustomQueryAsync(userId, section);
    }

    /// <inheritdoc />
    public async Task<List<HomeSectionDto>> GetBuiltInSectionsAsync(Guid userId)
    {
        var sections = new List<HomeSectionDto>
        {
            CreateBuiltIn(BuiltInSection.ContinueWatching, "Continuar viendo", CardViewType.Landscape, 1),
            CreateBuiltIn(BuiltInSection.LatestAdded, "Últimas añadidas", CardViewType.Portrait, 2),
            CreateBuiltIn(BuiltInSection.Favorites, "Mis favoritos", CardViewType.Portrait, 3),
            CreateBuiltIn(BuiltInSection.BecauseYouWatched, "Porque viste...", CardViewType.Portrait, 4),
            CreateBuiltIn(BuiltInSection.WatchAgain, "Ver otra vez", CardViewType.Portrait, 5),
            CreateBuiltIn(BuiltInSection.HiddenGems, "Joyas ocultas", CardViewType.Portrait, 6),
            CreateBuiltIn(BuiltInSection.TimeSlotSuggestions, "Sugerencias para ti", CardViewType.Thumb, 7),
        };

        foreach (var section in sections)
        {
            try
            {
                section.Items = await ExecuteBuiltInQueryAsync(userId, section).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating built-in section {SectionId}", section.SectionId);
            }
        }

        return sections.Where(s => s.Items.Count >= s.MinItems).ToList();
    }

    private static HomeSectionDto CreateBuiltIn(BuiltInSection type, string title, CardViewType cardType, int weight)
    {
        return new HomeSectionDto
        {
            SectionId = type.ToString(),
            Title = title,
            CardType = cardType,
            Weight = weight,
            BuiltInType = type,
            MinItems = type == BuiltInSection.ContinueWatching ? 1 : 0,
            MaxItems = 25,
        };
    }

    private Task<List<MediaCardDto>> ExecuteBuiltInQueryAsync(Guid userId, HomeSectionDto section)
    {
        return section.BuiltInType switch
        {
            BuiltInSection.ContinueWatching => GetContinueWatchingAsync(userId, section.MaxItems),
            BuiltInSection.LatestAdded => GetLatestAddedAsync(userId, section.MaxItems),
            BuiltInSection.Favorites => GetFavoritesAsync(userId, section.MaxItems),
            BuiltInSection.BecauseYouWatched => GetBecauseYouWatchedAsync(userId, section.MaxItems),
            BuiltInSection.WatchAgain => GetWatchAgainAsync(userId, section.MaxItems),
            BuiltInSection.HiddenGems => GetHiddenGemsAsync(userId, section.MaxItems),
            BuiltInSection.TimeSlotSuggestions => GetTimeSlotSuggestionsAsync(userId, section.MaxItems),
            _ => Task.FromResult(new List<MediaCardDto>()),
        };
    }

    private Task<List<MediaCardDto>> GetContinueWatchingAsync(Guid userId, int limit)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie, BaseItemKind.Episode],
            IsVirtualItem = false,
            IsResumable = true,
            Limit = limit,
            OrderBy = [(ItemSortBy.DatePlayed, SortOrder.Descending)],
        };
        SetUserFilter(query, userId);

        var items = _libraryManager.GetItemList(query);
        return Task.FromResult(items.Select(i => MapToCard(i, userId)).ToList());
    }

    private Task<List<MediaCardDto>> GetLatestAddedAsync(Guid userId, int limit)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie, BaseItemKind.Series],
            IsVirtualItem = false,
            Limit = limit,
            OrderBy = [(ItemSortBy.DateCreated, SortOrder.Descending)],
        };
        SetUserFilter(query, userId);

        var items = _libraryManager.GetItemList(query);
        return Task.FromResult(items.Select(i => MapToCard(i, userId)).ToList());
    }

    private Task<List<MediaCardDto>> GetFavoritesAsync(Guid userId, int limit)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie, BaseItemKind.Series],
            IsVirtualItem = false,
            IsFavorite = true,
            Limit = limit,
            OrderBy = [(ItemSortBy.SortName, SortOrder.Ascending)],
        };
        SetUserFilter(query, userId);

        var items = _libraryManager.GetItemList(query);
        return Task.FromResult(items.Select(i => MapToCard(i, userId)).ToList());
    }

    private Task<List<MediaCardDto>> GetBecauseYouWatchedAsync(Guid userId, int limit)
    {
        var recentQuery = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie],
            IsVirtualItem = false,
            IsPlayed = true,
            Limit = 1,
            OrderBy = [(ItemSortBy.DatePlayed, SortOrder.Descending)],
        };
        SetUserFilter(recentQuery, userId);

        var recentItems = _libraryManager.GetItemList(recentQuery);
        if (recentItems.Count == 0)
        {
            return Task.FromResult(new List<MediaCardDto>());
        }

        var lastWatched = recentItems[0];
        var genres = lastWatched.Genres;
        if (genres is null || genres.Length == 0)
        {
            return Task.FromResult(new List<MediaCardDto>());
        }

        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie],
            IsVirtualItem = false,
            IsPlayed = false,
            Genres = genres,
            Limit = limit,
            OrderBy = [(ItemSortBy.CommunityRating, SortOrder.Descending)],
        };
        SetUserFilter(query, userId);

        var items = _libraryManager.GetItemList(query)
            .Where(i => i.Id != lastWatched.Id)
            .ToList();

        return Task.FromResult(items.Select(i => MapToCard(i, userId)).ToList());
    }

    private Task<List<MediaCardDto>> GetWatchAgainAsync(Guid userId, int limit)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie],
            IsVirtualItem = false,
            IsPlayed = true,
            MinCommunityRating = 8.0,
            Limit = limit,
            OrderBy = [(ItemSortBy.DatePlayed, SortOrder.Ascending)],
        };
        SetUserFilter(query, userId);

        var items = _libraryManager.GetItemList(query);
        return Task.FromResult(items.Select(i => MapToCard(i, userId)).ToList());
    }

    private Task<List<MediaCardDto>> GetHiddenGemsAsync(Guid userId, int limit)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie],
            IsVirtualItem = false,
            IsPlayed = false,
            MinCommunityRating = 7.5,
            Limit = limit,
            OrderBy = [(ItemSortBy.CommunityRating, SortOrder.Descending)],
        };
        SetUserFilter(query, userId);

        var items = _libraryManager.GetItemList(query);
        return Task.FromResult(items.Select(i => MapToCard(i, userId)).ToList());
    }

    private Task<List<MediaCardDto>> GetTimeSlotSuggestionsAsync(Guid userId, int limit)
    {
        var hour = DateTime.Now.Hour;
        string[] genres = hour >= 22 || hour < 6
            ? ["Horror", "Thriller", "Terror"]
            : hour >= 6 && hour < 14
                ? ["Comedy", "Animation", "Family"]
                : ["Action", "Adventure", "Drama"];

        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie],
            IsVirtualItem = false,
            IsPlayed = false,
            Genres = genres,
            Limit = limit,
            OrderBy = [(ItemSortBy.CommunityRating, SortOrder.Descending)],
        };
        SetUserFilter(query, userId);

        var items = _libraryManager.GetItemList(query);
        return Task.FromResult(items.Select(i => MapToCard(i, userId)).ToList());
    }

    private Task<List<MediaCardDto>> ExecuteCustomQueryAsync(Guid userId, HomeSectionDto section)
    {
        var q = section.Query;
        if (q is null)
        {
            return Task.FromResult(new List<MediaCardDto>());
        }

        var itemTypes = q.MediaTypes
            .Select(t => Enum.TryParse<BaseItemKind>(t, true, out var kind) ? kind : (BaseItemKind?)null)
            .Where(k => k.HasValue)
            .Select(k => k!.Value)
            .ToArray();

        var query = new InternalItemsQuery
        {
            IncludeItemTypes = itemTypes,
            IsVirtualItem = false,
            Limit = section.MaxItems,
        };

        if (q.Genres.Count > 0)
        {
            query.Genres = q.Genres.ToArray();
        }

        if (q.Studios.Count > 0)
        {
            query.StudioIds = q.Studios
                .Select(s => _libraryManager.GetStudio(s))
                .Where(s => s is not null)
                .Select(s => s!.Id)
                .ToArray();
        }

        if (q.Tags.Count > 0)
        {
            query.Tags = q.Tags.ToArray();
        }

        if (q.MinYear.HasValue)
        {
            query.MinPremiereDate = new DateTime(q.MinYear.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        if (q.MaxYear.HasValue)
        {
            query.MaxPremiereDate = new DateTime(q.MaxYear.Value, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }

        if (q.MinRating.HasValue)
        {
            query.MinCommunityRating = q.MinRating;
        }

        var sortBy = q.SortBy switch
        {
            "CommunityRating" => ItemSortBy.CommunityRating,
            "ProductionYear" => ItemSortBy.ProductionYear,
            "Name" => ItemSortBy.SortName,
            "Random" => ItemSortBy.Random,
            _ => ItemSortBy.DateCreated,
        };

        var sortOrder = q.SortDirection == Models.SortDirection.Ascending
            ? SortOrder.Ascending
            : SortOrder.Descending;

        query.OrderBy = [(sortBy, sortOrder)];

        if (section.HideWatched == true)
        {
            query.IsPlayed = false;
        }

        SetUserFilter(query, userId);

        var items = _libraryManager.GetItemList(query);
        return Task.FromResult(items.Select(i => MapToCard(i, userId)).ToList());
    }

    private MediaCardDto MapToCard(BaseItem item, Guid userId)
    {
        var user = _userManager.GetUserById(userId);
        var userData = user is not null ? _userDataManager.GetUserData(user, item) : null;

        double? progress = null;
        if (userData?.PlaybackPositionTicks > 0 && item.RunTimeTicks > 0)
        {
            progress = (double)userData.PlaybackPositionTicks / item.RunTimeTicks.Value;
        }

        return new MediaCardDto
        {
            ItemId = item.Id,
            Title = item.Name,
            Subtitle = item.ProductionYear?.ToString(CultureInfo.InvariantCulture),
            PrimaryImageUrl = $"/Items/{item.Id}/Images/Primary",
            BackdropImageUrl = item.GetImages(MediaBrowser.Model.Entities.ImageType.Backdrop).Any()
                ? $"/Items/{item.Id}/Images/Backdrop"
                : null,
            MediaType = item.GetBaseItemKind().ToString(),
            Year = item.ProductionYear,
            CommunityRating = item.CommunityRating,
            OfficialRating = item.OfficialRating,
            PlaybackProgress = progress,
            IsPlayed = userData?.Played ?? false,
            IsFavorite = userData?.IsFavorite ?? false,
        };
    }

    private void SetUserFilter(InternalItemsQuery query, Guid userId)
    {
        var user = _userManager.GetUserById(userId);
        if (user is not null)
        {
            query.User = user;
        }
    }
}
