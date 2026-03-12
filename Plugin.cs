using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.UltimateHomeUI.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.UltimateHomeUI;

/// <summary>
/// Plugin principal de Jellyfin Ultimate Home UI.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "Ultimate Home UI";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("0736fda4-7484-4372-8ce0-9d959232d19c");

    /// <inheritdoc />
    public override string Description => "Reemplaza la pantalla de inicio de Jellyfin con una experiencia premium estilo Netflix, ultra-personalizable y responsive.";

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.Configuration.configPage.html",
                    GetType().Namespace),
            }
        ];
    }
}
