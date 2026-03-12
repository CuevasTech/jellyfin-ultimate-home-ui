using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.UltimateHomeUI.Models;

namespace Jellyfin.Plugin.UltimateHomeUI.Services;

/// <summary>
/// Ordena secciones por peso y resuelve empates según la estrategia configurada.
/// </summary>
public class SortingService : ISortingService
{
    /// <inheritdoc />
    public List<HomeSectionDto> SortSections(List<HomeSectionDto> sections, TieBreakingStrategy strategy)
    {
        var groups = sections
            .Where(s => s.IsVisible)
            .GroupBy(s => s.Weight)
            .OrderBy(g => g.Key);

        var result = new List<HomeSectionDto>();

        foreach (var group in groups)
        {
            var items = group.ToList();

            if (items.Count <= 1)
            {
                result.AddRange(items);
                continue;
            }

            switch (strategy)
            {
                case TieBreakingStrategy.Random:
                    FisherYatesShuffle(items);
                    break;
                case TieBreakingStrategy.Alphabetical:
                    items.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
                    break;
                case TieBreakingStrategy.LastActivity:
                    break;
                case TieBreakingStrategy.ServerDefault:
                default:
                    break;
            }

            result.AddRange(items);
        }

        return result;
    }

    private static void FisherYatesShuffle<T>(List<T> list)
    {
        var rng = Random.Shared;
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
