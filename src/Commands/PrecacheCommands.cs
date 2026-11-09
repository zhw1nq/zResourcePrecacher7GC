namespace zResourcePrecacher7GC.Commands;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using zResourcePrecacher7GC.Config;
using zResourcePrecacher7GC.Discord;
using zResourcePrecacher7GC.Reporting;
using zResourcePrecacher7GC.Services;

public sealed class PrecacheCommands
{
    private readonly ISwiftlyCore core;
    private readonly IPrecacheService precacheService;
    private readonly IDiscordWebhookService discordService;
    private readonly IOptionsMonitor<PluginConfig> configMonitor;

    public PrecacheCommands(
        ISwiftlyCore core,
        IPrecacheService precacheService,
        IDiscordWebhookService discordService,
        IOptionsMonitor<PluginConfig> configMonitor)
    {
        this.core = core;
        this.precacheService = precacheService;
        this.discordService = discordService;
        this.configMonitor = configMonitor;
    }

    [Command("precache_reload", false, "admin.precache.reload", "Reloads zResourcePrecacher7GC configuration and resources")]
    [CommandAlias("resprecache_reload", false)]
    public void OnPrecacheReloadCommand(ICommandContext context)
    {
        PluginConfig config = this.configMonitor.CurrentValue;

        if (context.IsSentByPlayer && context.Sender != null)
        {
            string requiredPermission = config.ReloadPermission;
            if (!string.IsNullOrWhiteSpace(requiredPermission) &&
                !this.core.Permission.PlayerHasPermission(context.Sender.SteamID, requiredPermission))
            {
                context.Reply($"{this.core.Localizer["prefix"]}{this.core.Localizer["command.no_permission"]}");
                return;
            }
        }

        context.Reply($"{this.core.Localizer["prefix"]}{this.core.Localizer["command.reload.start"]}");

        this.precacheService.Reload();

        context.Reply(
            $"{this.core.Localizer["prefix"]}{this.core.Localizer["command.reload.success", this.precacheService.PreparedResourceCount, this.precacheService.ScannedPackageCount]}");
    }

    [Command("precache_status", false, "admin.precache.status", "Displays zResourcePrecacher7GC current statistics")]
    [CommandAlias("resprecache_status", false)]
    public void OnPrecacheStatusCommand(ICommandContext context)
    {
        PluginConfig config = this.configMonitor.CurrentValue;

        if (context.IsSentByPlayer && context.Sender != null)
        {
            string requiredPermission = config.StatusPermission;
            if (!string.IsNullOrWhiteSpace(requiredPermission) &&
                !this.core.Permission.PlayerHasPermission(context.Sender.SteamID, requiredPermission))
            {
                context.Reply($"{this.core.Localizer["prefix"]}{this.core.Localizer["command.no_permission"]}");
                return;
            }
        }

        context.Reply(
            $"{this.core.Localizer["prefix"]}{this.core.Localizer["command.status.summary", this.precacheService.PreparedResourceCount, this.precacheService.ScannedPackageCount]}");

        string discordStatus = config.Discord.Enabled
            ? this.core.Localizer["command.status.discord_enabled"]
            : this.core.Localizer["command.status.discord_disabled"];

        context.Reply(
            $"{this.core.Localizer["prefix"]}{this.core.Localizer["command.status.discord", discordStatus]}");
    }

    [Command("precache_testwebhook", false, "admin.precache.reload", "Dispatches a test precache report to Discord webhook")]
    [CommandAlias("resprecache_testwebhook", false)]
    public void OnPrecacheTestWebhookCommand(ICommandContext context)
    {
        PluginConfig config = this.configMonitor.CurrentValue;

        if (context.IsSentByPlayer && context.Sender != null)
        {
            string requiredPermission = config.ReloadPermission;
            if (!string.IsNullOrWhiteSpace(requiredPermission) &&
                !this.core.Permission.PlayerHasPermission(context.Sender.SteamID, requiredPermission))
            {
                context.Reply($"{this.core.Localizer["prefix"]}{this.core.Localizer["command.no_permission"]}");
                return;
            }
        }

        if (!config.Discord.Enabled || string.IsNullOrWhiteSpace(config.Discord.WebhookUrl))
        {
            context.Reply($"{this.core.Localizer["prefix"]}{this.core.Localizer["command.webhook.disabled"]}");
            return;
        }

        context.Reply($"{this.core.Localizer["prefix"]}{this.core.Localizer["command.webhook.sending"]}");

        var report = new PrecacheReport
        {
            Timestamp = DateTimeOffset.UtcNow,
            ServerHostname = this.core.ConVar.FindAsString("hostname")?.ValueAsString ?? "Counter-Strike 2 Server",
            MapName = this.precacheService.CurrentMapName,
            ElapsedMilliseconds = 1
        };

        report.VpkPackages.Add(new PrecacheVpkSummary { PackageName = "test_package.vpk", EntryCount = 2 });
        report.Items.Add(new PrecacheReportItem("characters/models/test/test_agent.vmdl", PrecacheItemStatus.Pass));
        report.Items.Add(new PrecacheReportItem("soundevents/test/test_sound.vsndevts", PrecacheItemStatus.Pass));

        _ = Task.Run(async () =>
        {
            try
            {
                await this.discordService.DispatchReportAsync(report, config.Discord);
                context.Reply($"{this.core.Localizer["prefix"]}{this.core.Localizer["command.webhook.success"]}");
            }
            catch (Exception ex)
            {
                context.Reply($"{this.core.Localizer["prefix"]}{this.core.Localizer["command.webhook.failed", ex.Message]}");
            }
        });
    }
}
