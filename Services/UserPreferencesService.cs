using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.UltimateHomeUI.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.UltimateHomeUI.Services;

/// <summary>
/// Gestiona la persistencia de preferencias de usuario como archivos JSON individuales.
/// </summary>
public class UserPreferencesService : IUserPreferencesService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ILogger<UserPreferencesService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPreferencesService"/> class.
    /// </summary>
    public UserPreferencesService(ILogger<UserPreferencesService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<UserPreferencesDto?> GetPreferencesAsync(Guid userId)
    {
        var path = GetFilePath(userId);
        if (!File.Exists(path))
        {
            return null;
        }

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            return JsonSerializer.Deserialize<UserPreferencesDto>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading preferences for user");
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<UserPreferencesDto> UpdatePreferencesAsync(Guid userId, UserPreferencesDto preferences)
    {
        preferences.UserId = userId;
        preferences.HasCustomLayout = true;

        var path = GetFilePath(userId);
        var dir = Path.GetDirectoryName(path);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
        }

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var json = JsonSerializer.Serialize(preferences, JsonOptions);
            await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }

        return preferences;
    }

    /// <inheritdoc />
    public Task ResetPreferencesAsync(Guid userId)
    {
        var path = GetFilePath(userId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private static string GetFilePath(Guid userId)
    {
        var dataFolder = Plugin.Instance?.DataFolderPath
            ?? throw new InvalidOperationException("Plugin instance not available.");
        return Path.Combine(dataFolder, $"preferences_{userId:N}.json");
    }
}
