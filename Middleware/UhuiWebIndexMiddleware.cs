using System.Globalization;
using System.Text.RegularExpressions;
using MediaBrowser.Common.Configuration;
using Microsoft.AspNetCore.Http;

namespace Jellyfin.Plugin.UltimateHomeUI.Middleware;

/// <summary>
/// Intercepta las peticiones al index del cliente web y sirve index.html con el script
/// del plugin inyectado en memoria, sin depender de escritura en disco ni de File Transformation.
/// </summary>
public class UhuiWebIndexMiddleware
{
    private const string ScriptTagMarker = "plugin=\"UltimateHomeUI\"";
    private readonly RequestDelegate _next;

    /// <summary>Construye el middleware que intercepta el index del cliente web.</summary>
    /// <param name="next">Siguiente delegado en el pipeline.</param>
    public UhuiWebIndexMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>Ejecuta el middleware: sirve index.html inyectado si la ruta es /web o /web/index.html.</summary>
    /// <param name="context">Contexto HTTP de la petición.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsWebIndexRequest(context.Request.Path))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var appPaths = context.RequestServices.GetService(typeof(IApplicationPaths)) as IApplicationPaths;
        if (appPaths is null || string.IsNullOrWhiteSpace(appPaths.WebPath))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var indexFile = Path.Combine(appPaths.WebPath, "index.html");
        if (!File.Exists(indexFile))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        string indexContents;
        try
        {
            indexContents = await File.ReadAllTextAsync(indexFile, context.RequestAborted).ConfigureAwait(false);
        }
        catch
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var pathBase = context.Request.PathBase.HasValue ? context.Request.PathBase.Value!.TrimEnd('/') : string.Empty;
        var scriptTag = string.Format(
            CultureInfo.InvariantCulture,
            "<script type=\"module\" src=\"{0}/UltimateHomeUI/Web/plugin-entry.js\" plugin=\"UltimateHomeUI\"></script>",
            pathBase);

        var cleaned = Regex.Replace(
            indexContents,
            @"<script[^>]*plugin=""UltimateHomeUI""[^>]*>\s*</script>",
            string.Empty,
            RegexOptions.Singleline);

        if (!cleaned.Contains(ScriptTagMarker, StringComparison.Ordinal))
        {
            var bodyClose = cleaned.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (bodyClose >= 0)
            {
                cleaned = cleaned.Insert(bodyClose, scriptTag + "\n");
            }
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync(cleaned, context.RequestAborted).ConfigureAwait(false);
    }

    private static bool IsWebIndexRequest(PathString path)
    {
        var value = path.Value ?? string.Empty;
        var normalized = value.TrimEnd('/');
        return normalized.Equals("/web", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("/web/index.html", StringComparison.OrdinalIgnoreCase);
    }
}
