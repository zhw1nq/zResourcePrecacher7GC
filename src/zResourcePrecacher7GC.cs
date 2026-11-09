namespace zResourcePrecacher7GC;

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;
using zResourcePrecacher7GC.Commands;
using zResourcePrecacher7GC.Config;
using zResourcePrecacher7GC.Discord;
using zResourcePrecacher7GC.Events;
using zResourcePrecacher7GC.Reporting;
using zResourcePrecacher7GC.Services;
using zResourcePrecacher7GC.Vpk;

[PluginMetadata(
    Id = "zResourcePrecacher7GC",
    Version = "1.0.0",
    Name = "zResourcePrecacher7GC",
    Author = "vhming",
    Description = "zResourcePrecacher7GC")]
public partial class zResourcePrecacher7GCPlugin : BasePlugin
{
    private ServiceProvider? serviceProvider;
    private IDisposable? configChangeSubscription;

    public zResourcePrecacher7GCPlugin(ISwiftlyCore core) : base(core)
    {
    }

    public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
    {
    }

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
    }

    public override void Load(bool hotReload)
    {
        this.Core.Configuration
            .InitializeWithTemplate("config.jsonc", "template.jsonc")
            .Configure(builder =>
            {
                builder.AddJsonFile(
                    this.Core.Configuration.GetConfigPath("config.jsonc"),
                    optional: false,
                    reloadOnChange: true);
            });

        var services = new ServiceCollection();
        services.AddSwiftly(this.Core);
        services.AddOptionsWithValidateOnStart<PluginConfig>().BindConfiguration("zResourcePrecacher7GC");

        services.AddSingleton<IVpkArchiveReader, VpkArchiveReader>();
        services.AddSingleton<IPrecacheReporter, PrecacheReporter>();
        services.AddSingleton<IDiscordWebhookService, DiscordWebhookService>();
        services.AddSingleton<IPrecacheService, PrecacheService>();

        services.AddSingleton<PrecacheEventListener>();
        services.AddSingleton<PrecacheCommands>();

        this.serviceProvider = services.BuildServiceProvider();

        var eventListener = this.serviceProvider.GetRequiredService<PrecacheEventListener>();
        var commands = this.serviceProvider.GetRequiredService<PrecacheCommands>();

        this.Core.Registrator.Register(eventListener);
        this.Core.Registrator.Register(commands);

        var precacheService = this.serviceProvider.GetRequiredService<IPrecacheService>();
        precacheService.Reload();

        var configMonitor = this.serviceProvider.GetRequiredService<IOptionsMonitor<PluginConfig>>();
        this.configChangeSubscription = configMonitor.OnChange(_ =>
        {
            this.Core.Logger.LogInformation("Configuration reloaded. Re-indexing precache resources...");
            precacheService.Reload();
        });
    }

    public override void Unload()
    {
        this.configChangeSubscription?.Dispose();
        this.configChangeSubscription = null;

        this.serviceProvider?.Dispose();
        this.serviceProvider = null;
    }
}
