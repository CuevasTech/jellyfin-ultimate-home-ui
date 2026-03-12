using Jellyfin.Plugin.UltimateHomeUI.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.UltimateHomeUI;

/// <summary>
/// Registra los servicios del plugin en el contenedor de DI de Jellyfin.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IUserPreferencesService, UserPreferencesService>();
        serviceCollection.AddSingleton<ISortingService, SortingService>();
        serviceCollection.AddSingleton<ISectionQueryService, SectionQueryService>();
        serviceCollection.AddSingleton<IHeroService, HeroService>();

        // Inyecta el script del frontend en index.html al arrancar Jellyfin.
        serviceCollection.AddHostedService<WebInjectorService>();
    }
}
