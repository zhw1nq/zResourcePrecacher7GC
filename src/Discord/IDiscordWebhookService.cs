namespace zResourcePrecacher7GC.Discord;

using System.Threading;
using System.Threading.Tasks;
using zResourcePrecacher7GC.Config;
using zResourcePrecacher7GC.Reporting;

public interface IDiscordWebhookService
{
    Task DispatchReportAsync(PrecacheReport report, DiscordConfig config, CancellationToken cancellationToken = default);
}
