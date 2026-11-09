namespace zResourcePrecacher7GC.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using zResourcePrecacher7GC.Config;
using zResourcePrecacher7GC.Discord;
using zResourcePrecacher7GC.Reporting;
using zResourcePrecacher7GC.Vpk;

public sealed class PrecacheService : IPrecacheService
{
    private readonly ISwiftlyCore core;
    private readonly IVpkArchiveReader vpkReader;
    private readonly IDiscordWebhookService discordService;
    private readonly IOptionsMonitor<PluginConfig> configMonitor;
    private readonly ILogger<PrecacheService> logger;

    private readonly object syncLock = new();
    private readonly List<string> preparedResources = new();
    private readonly List<PrecacheVpkSummary> vpkSummaries = new();
    private readonly List<PrecacheReportItem> preparedReportItems = new();

    public int PreparedResourceCount
    {
        get
        {
            lock (this.syncLock)
            {
                return this.preparedResources.Count;
            }
        }
    }

    public int ScannedPackageCount
    {
        get
        {
            lock (this.syncLock)
            {
                return this.vpkSummaries.Count;
            }
        }
    }

    public string CurrentMapName { get; private set; } = "unknown";

    public PrecacheService(
        ISwiftlyCore core,
        IVpkArchiveReader vpkReader,
        IDiscordWebhookService discordService,
        IOptionsMonitor<PluginConfig> configMonitor,
        ILogger<PrecacheService> logger)
    {
        this.core = core;
        this.vpkReader = vpkReader;
        this.discordService = discordService;
        this.configMonitor = configMonitor;
        this.logger = logger;
    }

    public void SetCurrentMap(string mapName)
    {
        if (!string.IsNullOrWhiteSpace(mapName))
        {
            this.CurrentMapName = mapName;
        }
    }

    public void Reload()
    {
        PluginConfig config = this.configMonitor.CurrentValue;
        var uniqueSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var newResources = new List<string>();
        var newVpks = new List<PrecacheVpkSummary>();
        var newReportItems = new List<PrecacheReportItem>();

        string pluginBase = string.IsNullOrWhiteSpace(this.core.PluginPath)
            ? AppContext.BaseDirectory
            : this.core.PluginPath;

        string assetsDir = Path.Combine(pluginBase, config.AssetsDirectory);

        if (config.ScanAssetsDirectory)
        {
            if (!Directory.Exists(assetsDir))
            {
                try
                {
                    Directory.CreateDirectory(assetsDir);
                    this.logger.LogInformation("Created Assets directory at: '{Path}'", assetsDir);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to create Assets directory: {Message}", ex.Message);
                }
            }

            if (Directory.Exists(assetsDir))
            {
                foreach (string vpkPath in Directory.EnumerateFiles(assetsDir, "*.vpk", SearchOption.AllDirectories))
                {
                    if (vpkPath.EndsWith("_000.vpk", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string packageName = Path.GetFileName(vpkPath);
                    IReadOnlyList<string> entries = this.vpkReader.ReadResourcePaths(vpkPath, config.ResourceExtensions);
                    int addedFromThisVpk = 0;

                    foreach (string entry in entries)
                    {
                        string normalized = NormalizePath(entry);
                        if (uniqueSet.Add(normalized))
                        {
                            newResources.Add(normalized);
                            newReportItems.Add(new PrecacheReportItem(normalized, PrecacheItemStatus.Pass));
                            addedFromThisVpk++;
                        }
                        else
                        {
                            newReportItems.Add(new PrecacheReportItem(normalized, PrecacheItemStatus.Warn, "Duplicate resource skipped"));
                        }
                    }

                    newVpks.Add(new PrecacheVpkSummary
                    {
                        PackageName = packageName,
                        EntryCount = addedFromThisVpk
                    });

                    this.logger.LogInformation("Indexed VPK '{PackageName}' with {Count} resource entries.", packageName, addedFromThisVpk);
                }
            }
        }

        if (config.Resources.Count > 0)
        {
            foreach (string rawPath in config.Resources)
            {
                if (string.IsNullOrWhiteSpace(rawPath))
                {
                    continue;
                }

                string normalized = NormalizePath(rawPath);
                string ext = Path.GetExtension(normalized).TrimStart('.');

                if (!config.ResourceExtensions.Contains(ext))
                {
                    string reason = $"Unsupported extension: '{ext}'";
                    this.logger.LogWarning("Resource '{Path}' skipped: {Reason}", rawPath, reason);
                    newReportItems.Add(new PrecacheReportItem(normalized, PrecacheItemStatus.Warn, reason));
                    continue;
                }

                if (uniqueSet.Add(normalized))
                {
                    newResources.Add(normalized);
                    newReportItems.Add(new PrecacheReportItem(normalized, PrecacheItemStatus.Pass));
                }
                else
                {
                    newReportItems.Add(new PrecacheReportItem(normalized, PrecacheItemStatus.Warn, "Duplicate resource in configuration skipped"));
                }
            }
        }

        lock (this.syncLock)
        {
            this.preparedResources.Clear();
            this.preparedResources.AddRange(newResources);

            this.vpkSummaries.Clear();
            this.vpkSummaries.AddRange(newVpks);

            this.preparedReportItems.Clear();
            this.preparedReportItems.AddRange(newReportItems);
        }

        this.logger.LogInformation("Prepared {Total} resources across {VpkCount} VPK package(s) and configuration.", newResources.Count, newVpks.Count);
    }

    public void ExecutePrecache(IOnPrecacheResourceEvent @event)
    {
        PluginConfig config = this.configMonitor.CurrentValue;
        var stopwatch = Stopwatch.StartNew();

        List<string> resourcesToPrecache;
        List<PrecacheVpkSummary> vpks;
        List<PrecacheReportItem> reportItems;

        lock (this.syncLock)
        {
            resourcesToPrecache = new List<string>(this.preparedResources);
            vpks = new List<PrecacheVpkSummary>(this.vpkSummaries);
            reportItems = new List<PrecacheReportItem>(this.preparedReportItems);
        }

        int precachedCount = 0;
        foreach (string path in resourcesToPrecache)
        {
            @event.AddItem(path);
            precachedCount++;

            if (config.Log && config.VerboseLog)
            {
                this.logger.LogInformation("Precaching \"{Resource}\" [{Index}/{Total}]", path, precachedCount, resourcesToPrecache.Count);
            }
        }

        stopwatch.Stop();

        string hostname = this.core.ConVar.FindAsString("hostname")?.ValueAsString ?? "Counter-Strike 2 Server";
        string activeMap = !string.IsNullOrWhiteSpace(this.CurrentMapName) && this.CurrentMapName != "unknown"
            ? this.CurrentMapName
            : (this.core.Engine.GlobalVars.MapName.ToString() ?? "unknown");

        if (config.Log)
        {
            this.logger.LogInformation("Precached {Count} resources in {Elapsed}ms for map '{Map}'.", precachedCount, stopwatch.ElapsedMilliseconds, activeMap);
        }

        var report = new PrecacheReport
        {
            Timestamp = DateTimeOffset.UtcNow,
            ServerHostname = hostname,
            MapName = activeMap,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
        };

        report.VpkPackages.AddRange(vpks);
        report.Items.AddRange(reportItems);

        if (config.Discord.Enabled || config.Discord.SaveLocalLogFile)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await this.discordService.DispatchReportAsync(report, config.Discord);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Background Discord dispatch error: {Message}", ex.Message);
                }
            });
        }
    }

    private static string NormalizePath(string path)
    {
        string normalized = path.Replace('\\', '/').Trim().TrimStart('/');
        if (normalized.EndsWith("_c", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^2];
        }

        return normalized;
    }
}
