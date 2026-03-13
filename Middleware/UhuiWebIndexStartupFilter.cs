using Jellyfin.Plugin.UltimateHomeUI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Jellyfin.Plugin.UltimateHomeUI.Middleware;

/// <summary>
/// Registra el middleware que sirve index.html con el script inyectado al inicio del pipeline,
/// de modo que no se dependa de File Transformation ni de permisos de escritura en disco.
/// </summary>
public class UhuiWebIndexStartupFilter : IStartupFilter
{
    /// <inheritdoc />
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<UhuiWebIndexMiddleware>();
            Plugin.MiddlewareInjectionActive = true;
            next(app);
        };
    }
}
