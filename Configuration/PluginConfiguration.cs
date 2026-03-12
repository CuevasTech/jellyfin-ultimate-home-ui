using Jellyfin.Plugin.UltimateHomeUI.Models;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.UltimateHomeUI.Configuration;

/// <summary>
/// Configuración global del plugin (compartida por todos los usuarios).
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        EnableCustomLayout = true;
        DefaultTieBreakingStrategy = TieBreakingStrategy.Random;
        MaxSectionsVisible = 15;
        HeroSliderEnabled = true;
        HeroSliderSource = HeroSource.LatestAdded;
        HeroSliderMaxItems = 5;
        HideWatchedGlobally = false;
    }

    /// <summary>
    /// Gets or sets a value indicating whether custom layout is enabled globally.
    /// </summary>
    public bool EnableCustomLayout { get; set; }

    /// <summary>
    /// Gets or sets the default tie-breaking strategy for sections with equal weight.
    /// </summary>
    public TieBreakingStrategy DefaultTieBreakingStrategy { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of sections visible on the home screen.
    /// </summary>
    public int MaxSectionsVisible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the hero slider is enabled.
    /// </summary>
    public bool HeroSliderEnabled { get; set; }

    /// <summary>
    /// Gets or sets the content source for the hero slider.
    /// </summary>
    public HeroSource HeroSliderSource { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items in the hero slider rotation.
    /// </summary>
    public int HeroSliderMaxItems { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether watched content is hidden globally.
    /// </summary>
    public bool HideWatchedGlobally { get; set; }
}
