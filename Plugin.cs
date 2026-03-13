using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
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
    private const string ScriptTagMarker = "plugin=\"UltimateHomeUI\"";
    private static readonly Guid FileTransformationRegistrationId = Guid.Parse("55f6c896-6150-4f8b-926f-54e8dcf7dbd1");

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
            RegisterFileTransformationInjection();
            // Fallback para servidores sin File Transformation instalado.
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

    /// <summary>Gets a value indicating whether index.html was patched on disk.</summary>
    public static bool IndexInjectionActive { get; private set; }

    /// <summary>Gets a value indicating whether runtime File Transformation injection is active.</summary>
    public static bool FileTransformationInjectionActive { get; private set; }

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
                IndexInjectionActive = true;
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
            IndexInjectionActive = true;
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

    /// <summary>
    /// Registra una transformación de index.html en runtime si el plugin
    /// "File Transformation" está instalado. Evita depender de permisos de escritura en disco.
    /// </summary>
    private void RegisterFileTransformationInjection()
    {
        try
        {
            var ftAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.FullName?.Contains(".FileTransformation", StringComparison.OrdinalIgnoreCase) == true);

            if (ftAssembly is null)
            {
                _logger.LogInformation("[UHUI] File Transformation no instalado. Se usará inyección en disco como fallback.");
                return;
            }

            var pluginInterfaceType = ftAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
            var registerMethod = pluginInterfaceType?.GetMethod("RegisterTransformation", BindingFlags.Public | BindingFlags.Static);
            if (registerMethod is null)
            {
                _logger.LogWarning("[UHUI] File Transformation detectado pero no se encontró RegisterTransformation.");
                return;
            }

            var jObjectType = ftAssembly.GetType("Newtonsoft.Json.Linq.JObject")
                ?? AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Newtonsoft.Json.Linq.JObject")).FirstOrDefault(t => t is not null);
            if (jObjectType is null)
            {
                _logger.LogWarning("[UHUI] No se pudo resolver Newtonsoft.Json.Linq.JObject para registrar transformación.");
                return;
            }

            var parseMethod = jObjectType.GetMethod("Parse", [typeof(string)]);
            if (parseMethod is null)
            {
                _logger.LogWarning("[UHUI] No se pudo resolver JObject.Parse(string).");
                return;
            }

            var payloadJson = "{"
                + $"\"id\":\"{FileTransformationRegistrationId}\","
                + "\"fileNamePattern\":\"index\\\\.html$\","
                + $"\"callbackAssembly\":\"{EscapeJson(typeof(Plugin).Assembly.FullName ?? string.Empty)}\","
                + $"\"callbackClass\":\"{EscapeJson(typeof(Plugin).FullName ?? string.Empty)}\","
                + $"\"callbackMethod\":\"{nameof(TransformIndexHtml)}\""
                + "}";

            var payloadObj = parseMethod.Invoke(null, [payloadJson]);
            registerMethod.Invoke(null, [payloadObj]);

            FileTransformationInjectionActive = true;
            _logger.LogInformation("[UHUI] File Transformation registrado correctamente para index.html.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UHUI] Error registrando File Transformation.");
        }
    }

    /// <summary>
    /// Callback invocado por jellyfin-plugin-file-transformation para parchear index.html en memoria.
    /// </summary>
    /// <param name="payload">Objeto con propiedad 'contents'.</param>
    /// <returns>HTML transformado.</returns>
    public static string TransformIndexHtml(object payload)
    {
        var contents = GetPayloadContents(payload);
        if (string.IsNullOrEmpty(contents))
        {
            return contents;
        }

        var scriptTag = "<script type=\"module\" src=\"/UltimateHomeUI/Web/plugin-entry.js\" plugin=\"UltimateHomeUI\"></script>";

        // Limpiar cualquier registro previo del mismo plugin para evitar duplicados.
        var cleaned = Regex.Replace(
            contents,
            @"<script[^>]*plugin=""UltimateHomeUI""[^>]*>\s*</script>",
            string.Empty,
            RegexOptions.Singleline);

        if (cleaned.Contains(ScriptTagMarker, StringComparison.Ordinal))
        {
            return cleaned;
        }

        var bodyClose = cleaned.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (bodyClose < 0)
        {
            return cleaned;
        }

        return cleaned.Insert(bodyClose, scriptTag + "\n");
    }

    private static string GetPayloadContents(object payload)
    {
        if (payload is null)
        {
            return string.Empty;
        }

        var type = payload.GetType();

        // Soporte JObject dinámico (payload["contents"])
        try
        {
            var itemProp = type.GetProperty("Item", [typeof(object)]);
            if (itemProp is not null)
            {
                var token = itemProp.GetValue(payload, ["contents"]);
                var tokenType = token?.GetType();
                var tokenToString = tokenType?.GetMethod("ToString", Type.EmptyTypes);
                if (tokenToString is not null)
                {
                    var value = tokenToString.Invoke(token, null)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }
        }
        catch
        {
            // Ignore and continue.
        }

        // Soporte POCO payload.contents
        var contentsProp = type.GetProperty("contents", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return contentsProp?.GetValue(payload)?.ToString() ?? string.Empty;
    }

    private static string EscapeJson(string input)
        => input.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
}
