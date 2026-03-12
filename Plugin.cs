using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        try
        {
            EnsureConfigurationDefaults();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UHUI] Error al inicializar defaults de configuración.");
        }

        try
        {
            InjectClientScript(applicationPaths, configurationManager);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UHUI] Error al inyectar script en index.html.");
        }
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
    /// Garantiza que la configuración tenga valores por defecto válidos y sin duplicados.
    /// Se llama una vez al arrancar Jellyfin.
    /// El bug: XmlSerializer llama al constructor (que inicializaría la lista) y LUEGO
    /// deserializa el XML AÑADIENDO items al List existente en lugar de reemplazarlo.
    /// Por eso el constructor NO pre-puebla las listas — solo aquí se inicializan defaults.
    /// </summary>
    private void EnsureConfigurationDefaults()
    {
        var config = Configuration;
        var needsSave = false;

        // El XmlSerializer puede dejar las listas como null si el XML no las contiene
        config.Sections ??= [];
        config.Tabs ??= [];

        if (config.Sections.Count == 0)
        {
            config.Sections = PluginConfiguration.GetDefaultSections();
            needsSave = true;
            _logger.LogInformation("[UHUI] Secciones por defecto inicializadas.");
        }

        if (config.Tabs.Count == 0)
        {
            config.Tabs = PluginConfiguration.GetDefaultTabs();
            needsSave = true;
            _logger.LogInformation("[UHUI] Pestañas por defecto inicializadas.");
        }

        // Deduplicar listas corruptas (bug del XmlSerializer que añade en vez de reemplazar)
        var uniqueSections = config.Sections
            .Where(s => !string.IsNullOrEmpty(s?.SectionId))
            .GroupBy(s => s.SectionId)
            .Select(g => g.Last())
            .ToList();

        if (uniqueSections.Count != config.Sections.Count)
        {
            _logger.LogWarning(
                "[UHUI] Secciones duplicadas detectadas ({Total} → {Unique}). Deduplicando.",
                config.Sections.Count,
                uniqueSections.Count);
            config.Sections = uniqueSections;
            needsSave = true;
        }

        var uniqueTabs = config.Tabs
            .Where(t => !string.IsNullOrEmpty(t?.TabId))
            .GroupBy(t => t.TabId)
            .Select(g => g.Last())
            .ToList();

        if (uniqueTabs.Count != config.Tabs.Count)
        {
            _logger.LogWarning(
                "[UHUI] Pestañas duplicadas detectadas ({Total} → {Unique}). Deduplicando.",
                config.Tabs.Count,
                uniqueTabs.Count);
            config.Tabs = uniqueTabs;
            needsSave = true;
        }

        if (needsSave)
        {
            SaveConfiguration();
        }
    }

    /// <summary>
    /// Inyecta el módulo JS del frontend en el index.html de Jellyfin.
    /// Usa el atributo plugin="UltimateHomeUI" para identificar y eliminar inyecciones previas
    /// antes de reinsertar, garantizando idempotencia y correcta actualización del tag.
    /// </summary>
    private void InjectClientScript(IApplicationPaths applicationPaths, IServerConfigurationManager configurationManager)
    {
        if (string.IsNullOrWhiteSpace(applicationPaths.WebPath))
        {
            _logger.LogWarning("[UHUI] WebPath no configurado — inyección del script omitida.");
            return;
        }

        var indexFile = Path.Combine(applicationPaths.WebPath, "index.html");
        if (!File.Exists(indexFile))
        {
            _logger.LogWarning("[UHUI] index.html no encontrado en {IndexFile}", indexFile);
            return;
        }

        // Obtener el BasePath configurado para rutas con subdirectorio (ej: /jellyfin)
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
            _logger.LogError(ex, "[UHUI] No se pudo leer BaseUrl de la configuración de red.");
        }

        var scriptTag = string.Format(
            CultureInfo.InvariantCulture,
            "<script type=\"module\" src=\"{0}/UltimateHomeUI/Web/plugin-entry.js\" plugin=\"UltimateHomeUI\"></script>",
            basePath);

        try
        {
            var indexContents = File.ReadAllText(indexFile);

            // Eliminar cualquier inyección previa para evitar duplicados al actualizar
            indexContents = Regex.Replace(
                indexContents,
                @"<script[^>]*plugin=""UltimateHomeUI""[^>]*>\s*</script>",
                string.Empty,
                RegexOptions.Singleline);

            // Evitar inyección doble si el tag ya está presente exactamente igual
            if (indexContents.Contains(scriptTag, StringComparison.Ordinal))
            {
                _logger.LogInformation("[UHUI] Script ya presente en {IndexFile}", indexFile);
                return;
            }

            var bodyClose = indexContents.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (bodyClose < 0)
            {
                _logger.LogWarning("[UHUI] No se encontró </body> en {IndexFile}", indexFile);
                return;
            }

            indexContents = indexContents.Insert(bodyClose, scriptTag + "\n");
            File.WriteAllText(indexFile, indexContents);
            _logger.LogInformation("[UHUI] Script inyectado en {IndexFile}", indexFile);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(
                ex,
                "[UHUI] Permiso denegado al escribir en {IndexFile}. En Docker, asegúrate de que el directorio web sea escribible.",
                indexFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UHUI] Error al inyectar el script en {IndexFile}", indexFile);
        }
    }
}
