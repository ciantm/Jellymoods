using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JellyMoods;

/// <summary>
/// Patches web/config.json on startup to globally load sidebar.js.
/// </summary>
public class ServerEntryPoint : IServerEntryPoint
{
    private const string ScriptSrc  = "/JellyMoods/sidebar.js";
    private const string ScriptType = "module";

    private readonly ILogger<ServerEntryPoint> _logger;

    /// <summary>Initializes a new instance of <see cref="ServerEntryPoint"/>.</summary>
    public ServerEntryPoint(ILogger<ServerEntryPoint> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task RunAsync(CancellationToken cancellationToken)
    {
        try { PatchWebConfig(); }
        catch (Exception ex) { _logger.LogWarning(ex, "[JellyMoods] Could not patch config.json"); }
        return Task.CompletedTask;
    }

    private void PatchWebConfig()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "jellyfin-web", "config.json"),
            Path.Combine(baseDir, "web", "config.json"),
            "/usr/share/jellyfin/web/config.json",
            "/usr/lib/jellyfin/bin/jellyfin-web/config.json",
            "/usr/share/jellyfin-web/config.json",
        };

        string? configPath = null;
        foreach (var c in candidates) { if (File.Exists(c)) { configPath = c; break; } }

        if (configPath is null)
        {
            _logger.LogWarning("[JellyMoods] config.json not found");
            return;
        }

        var json = JsonNode.Parse(File.ReadAllText(configPath)) as JsonObject ?? new JsonObject();

        if (json["plugins"] is not JsonArray plugins)
        {
            plugins = new JsonArray();
            json["plugins"] = plugins;
        }

        foreach (var node in plugins)
        {
            if (node is JsonObject obj && obj["src"]?.GetValue<string>() == ScriptSrc)
            {
                _logger.LogInformation("[JellyMoods] sidebar.js already registered");
                return;
            }
        }

        plugins.Add(new JsonObject { ["src"] = ScriptSrc, ["type"] = ScriptType });
        File.WriteAllText(configPath, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        _logger.LogInformation("[JellyMoods] Registered sidebar.js in config.json");
    }

    /// <inheritdoc />
    public void Dispose() { }
}
