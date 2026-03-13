using Jellyfin.Plugin.UltimateHomeUI.Middleware;
using Jellyfin.Plugin.UltimateHomeUI.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.AspNetCore.Hosting;
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
        serviceCollection.AddSingleton<IStartupFilter, UhuiWebIndexStartupFilter>();
    }
}
