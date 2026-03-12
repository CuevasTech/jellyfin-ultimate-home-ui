using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.UltimateHomeUI.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.UltimateHomeUI;

/// <summary>
/// Plugin principal de Jellyfin Ultimate Home UI.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly ILogger<Plugin> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILogger<Plugin> logger,
        IServerConfigurationManager configurationManager)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _logger = logger;

        InjectClientScript(applicationPaths, configurationManager);
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
        var resourcePath = string.Format(
            CultureInfo.InvariantCulture,
            "{0}.Configuration.configPage.html",
            GetType().Namespace);

        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = resourcePath,
            },
            new PluginPageInfo
            {
                Name = "Ultimate Home UI Config",
                EmbeddedResourcePath = resourcePath,
                EnableInMainMenu = true,
                MenuSection = "server",
                MenuIcon = "dashboard",
                DisplayName = "Ultimate Home UI",
            }
        ];
    }

    /// <summary>
    /// Injects the client-side module script into Jellyfin's index.html.
    /// This is the standard approach used by plugins that need to extend the web UI
    /// (same pattern as jellyfin-plugin-custom-javascript).
    /// </summary>
    private void InjectClientScript(IApplicationPaths applicationPaths, IServerConfigurationManager configurationManager)
    {
        if (string.IsNullOrWhiteSpace(applicationPaths.WebPath))
        {
            _logger.LogWarning("[UHUI] WebPath is not set — client script injection skipped");
            return;
        }

        var indexFile = Path.Combine(applicationPaths.WebPath, "index.html");
        if (!File.Exists(indexFile))
        {
            _logger.LogWarning("[UHUI] index.html not found at {IndexFile}", indexFile);
            return;
        }

        var basePath = string.Empty;
        try
        {
            var networkConfig = configurationManager.GetConfiguration("network");
            var baseUrlProperty = networkConfig.GetType().GetProperty("BaseUrl");
            var confBasePath = baseUrlProperty?.GetValue(networkConfig)?.ToString()?.Trim('/');
            if (!string.IsNullOrEmpty(confBasePath))
            {
                basePath = "/" + confBasePath;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UHUI] Could not read base path from network config");
        }

        var scriptTag = string.Format(
            CultureInfo.InvariantCulture,
            "<script type=\"module\" src=\"{0}/UltimateHomeUI/Web/plugin-entry.js\" plugin=\"UltimateHomeUI\"></script>",
            basePath);

        try
        {
            var indexContents = File.ReadAllText(indexFile);

            indexContents = Regex.Replace(
                indexContents,
                @"<script[^>]*plugin=""UltimateHomeUI""[^>]*>[^<]*</script>",
                string.Empty,
                RegexOptions.Singleline);

            if (indexContents.Contains(scriptTag, StringComparison.Ordinal))
            {
                _logger.LogInformation("[UHUI] Client script already present in {IndexFile}", indexFile);
                return;
            }

            var bodyClose = indexContents.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (bodyClose < 0)
            {
                _logger.LogWarning("[UHUI] Could not find </body> tag in {IndexFile}", indexFile);
                return;
            }

            indexContents = indexContents.Insert(bodyClose, scriptTag + "\n");
            File.WriteAllText(indexFile, indexContents);
            _logger.LogInformation("[UHUI] Client script injected into {IndexFile}", indexFile);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "[UHUI] Permission denied writing to {IndexFile}. If running in Docker, ensure the web directory is writable.", indexFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UHUI] Failed to inject client script into {IndexFile}", indexFile);
        }
    }
}
