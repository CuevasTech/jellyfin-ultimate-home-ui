using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.UltimateHomeUI.Services;

/// <summary>
/// Servicio que inyecta el script del frontend en el index.html de Jellyfin al arrancar,
/// garantizando que el interceptor de la Home se cargue en cada visita del usuario.
/// </summary>
public class WebInjectorService : IHostedService
{
    // Marcador único que permite detectar si ya se inyectó y facilitar la eliminación limpia.
    private const string ScriptMarker = "<!-- UltimateHomeUI:inject -->";

    private const string ScriptTag =
        "<script type=\"module\" src=\"/UltimateHomeUI/Web/plugin-entry.js\"></script>";

    private readonly IApplicationPaths _applicationPaths;
    private readonly ILogger<WebInjectorService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebInjectorService"/> class.
    /// </summary>
    public WebInjectorService(IApplicationPaths applicationPaths, ILogger<WebInjectorService> logger)
    {
        _applicationPaths = applicationPaths;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            InjectScript();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UltimateHomeUI] Error al inyectar el script en index.html");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            RemoveScript();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UltimateHomeUI] Error al eliminar el script de index.html");
        }

        return Task.CompletedTask;
    }

    private string GetIndexHtmlPath()
        => Path.Combine(_applicationPaths.WebPath, "index.html");

    private void InjectScript()
    {
        var indexPath = GetIndexHtmlPath();

        if (!File.Exists(indexPath))
        {
            _logger.LogWarning("[UltimateHomeUI] index.html no encontrado en {Path}. El frontend web de Jellyfin puede no estar instalado.", indexPath);
            return;
        }

        var content = File.ReadAllText(indexPath);

        if (content.Contains(ScriptMarker, StringComparison.Ordinal))
        {
            _logger.LogDebug("[UltimateHomeUI] El script ya está inyectado en index.html, omitiendo.");
            return;
        }

        var injection = $"\n    {ScriptMarker}\n    {ScriptTag}";
        var modified = content.Replace("</body>", injection + "\n</body>", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(modified, content, StringComparison.Ordinal))
        {
            _logger.LogWarning("[UltimateHomeUI] No se encontró </body> en index.html. No se pudo inyectar el script.");
            return;
        }

        File.WriteAllText(indexPath, modified);
        _logger.LogInformation("[UltimateHomeUI] Script inyectado correctamente en {Path}", indexPath);
    }

    private void RemoveScript()
    {
        var indexPath = GetIndexHtmlPath();

        if (!File.Exists(indexPath))
        {
            return;
        }

        var content = File.ReadAllText(indexPath);

        if (!content.Contains(ScriptMarker, StringComparison.Ordinal))
        {
            return;
        }

        var injection = $"\n    {ScriptMarker}\n    {ScriptTag}";
        var modified = content.Replace(injection, string.Empty, StringComparison.Ordinal);

        File.WriteAllText(indexPath, modified);
        _logger.LogInformation("[UltimateHomeUI] Script eliminado de {Path}", indexPath);
    }
}
