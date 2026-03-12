using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Jellyfin.Plugin.UltimateHomeUI.Models;
using Jellyfin.Plugin.UltimateHomeUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.UltimateHomeUI.Controllers;

/// <summary>
/// Sirve la pantalla de inicio completa con secciones ordenadas y Hero Slider.
/// </summary>
[ApiController]
[Route("Plugins/UltimateHomeUI")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class HomeController : ControllerBase
{
    private readonly ISectionQueryService _sectionQueryService;
    private readonly ISortingService _sortingService;
    private readonly IHeroService _heroService;
    private readonly IUserPreferencesService _preferencesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    public HomeController(
        ISectionQueryService sectionQueryService,
        ISortingService sortingService,
        IHeroService heroService,
        IUserPreferencesService preferencesService)
    {
        _sectionQueryService = sectionQueryService;
        _sortingService = sortingService;
        _heroService = heroService;
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Obtiene la pantalla de inicio completa para un usuario.
    /// </summary>
    [HttpGet("Home/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<HomePageDto>> GetHomePage([FromRoute] Guid userId)
    {
        var config = Plugin.Instance?.Configuration;
        var prefs = await _preferencesService.GetPreferencesAsync(userId).ConfigureAwait(false);

        var strategy = prefs?.TieBreakingStrategy
            ?? config?.DefaultTieBreakingStrategy
            ?? TieBreakingStrategy.Random;

        var heroSource = prefs?.HeroSource
            ?? config?.HeroSliderSource
            ?? HeroSource.LatestAdded;

        var heroMaxItems = config?.HeroSliderMaxItems ?? 5;

        var heroItems = (config?.HeroSliderEnabled ?? true)
            ? await _heroService.GetHeroItemsAsync(userId, heroSource, heroMaxItems).ConfigureAwait(false)
            : new List<HeroItemDto>();

        var sections = await _sectionQueryService.GetBuiltInSectionsAsync(userId).ConfigureAwait(false);

        if (prefs?.SectionWeights is { Count: > 0 })
        {
            foreach (var weight in prefs.SectionWeights)
            {
                var section = sections.Find(s => s.SectionId == weight.SectionId);
                if (section is not null)
                {
                    section.Weight = weight.Weight;
                    section.IsVisible = weight.IsVisible;
                }
            }
        }

        var maxSections = config?.MaxSectionsVisible ?? 15;
        var sortedSections = _sortingService.SortSections(sections, strategy);
        if (sortedSections.Count > maxSections)
        {
            sortedSections = sortedSections.GetRange(0, maxSections);
        }

        return Ok(new HomePageDto
        {
            HeroItems = heroItems,
            Sections = sortedSections,
            Tabs = prefs?.Tabs ?? new List<HomeTabDto>(),
        });
    }
}

/// <summary>
/// DTO completo de la pantalla de inicio.
/// </summary>
public class HomePageDto
{
    /// <summary>Gets or sets the hero slider items.</summary>
    public List<HeroItemDto> HeroItems { get; set; } = [];

    /// <summary>Gets or sets the sorted sections with media cards.</summary>
    public List<HomeSectionDto> Sections { get; set; } = [];

    /// <summary>Gets or sets the navigation tabs.</summary>
    public List<HomeTabDto> Tabs { get; set; } = [];
}
