using System;
using System.Collections.Generic;
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
/// Proporciona contenido para el Hero Slider de la cabecera principal.
/// </summary>
public class HeroService : IHeroService
{
    private readonly ILibraryManager _libraryManager;
    private readonly IUserManager _userManager;
    private readonly IUserDataManager _userDataManager;
    private readonly ILogger<HeroService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroService"/> class.
    /// </summary>
    public HeroService(
        ILibraryManager libraryManager,
        IUserManager userManager,
        IUserDataManager userDataManager,
        ILogger<HeroService> logger)
    {
        _libraryManager = libraryManager;
        _userManager = userManager;
        _userDataManager = userDataManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<List<HeroItemDto>> GetHeroItemsAsync(Guid userId, HeroSource source, int maxItems)
    {
        try
        {
            var items = source switch
            {
                HeroSource.LatestAdded => GetLatest(userId, maxItems),
                HeroSource.Random => GetRandom(userId, maxItems),
                HeroSource.Trending => GetTrending(userId, maxItems),
                _ => GetLatest(userId, maxItems),
            };

            return Task.FromResult(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching hero items");
            return Task.FromResult(new List<HeroItemDto>());
        }
    }

    private List<HeroItemDto> GetLatest(Guid userId, int limit)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie, BaseItemKind.Series],
            IsVirtualItem = false,
            Limit = limit,
            OrderBy = [(ItemSortBy.DateCreated, SortOrder.Descending)],
        };
        SetUserFilter(query, userId);

        return _libraryManager.GetItemList(query).Select(i => MapToHero(i, userId)).ToList();
    }

    private List<HeroItemDto> GetRandom(Guid userId, int limit)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie, BaseItemKind.Series],
            IsVirtualItem = false,
            MinCommunityRating = 6.0,
            Limit = limit,
            OrderBy = [(ItemSortBy.Random, SortOrder.Descending)],
        };
        SetUserFilter(query, userId);

        return _libraryManager.GetItemList(query).Select(i => MapToHero(i, userId)).ToList();
    }

    private List<HeroItemDto> GetTrending(Guid userId, int limit)
    {
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie, BaseItemKind.Series],
            IsVirtualItem = false,
            Limit = limit,
            OrderBy = [(ItemSortBy.DatePlayed, SortOrder.Descending)],
        };
        SetUserFilter(query, userId);

        return _libraryManager.GetItemList(query).Select(i => MapToHero(i, userId)).ToList();
    }

    private HeroItemDto MapToHero(BaseItem item, Guid userId)
    {
        var user = _userManager.GetUserById(userId);
        var userData = user is not null ? _userDataManager.GetUserData(user, item) : null;

        var overview = item.Overview;
        if (overview is not null && overview.Length > 200)
        {
            overview = string.Concat(overview.AsSpan(0, 197), "...");
        }

        int? runtimeMinutes = null;
        if (item.RunTimeTicks.HasValue)
        {
            runtimeMinutes = (int)(item.RunTimeTicks.Value / TimeSpan.TicksPerMinute);
        }

        var hasBackdrop = item.GetImages(MediaBrowser.Model.Entities.ImageType.Backdrop).Any();
        var hasLogo = item.GetImages(MediaBrowser.Model.Entities.ImageType.Logo).Any();

        string? trailerUrl = null;
        if (item.RemoteTrailers is { Count: > 0 })
        {
            trailerUrl = item.RemoteTrailers[0].Url;
        }

        return new HeroItemDto
        {
            ItemId = item.Id,
            Title = item.Name,
            Tagline = item.Tagline,
            Overview = overview,
            Year = item.ProductionYear,
            OfficialRating = item.OfficialRating,
            CommunityRating = item.CommunityRating,
            RuntimeMinutes = runtimeMinutes,
            Genres = item.Genres is { Length: > 0 } ? string.Join(", ", item.Genres) : null,
            BackdropUrl = hasBackdrop ? $"/Items/{item.Id}/Images/Backdrop" : null,
            LogoUrl = hasLogo ? $"/Items/{item.Id}/Images/Logo" : null,
            TrailerUrl = trailerUrl,
            MediaType = item.GetBaseItemKind().ToString(),
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
