namespace zResourcePrecacher7GC.Discord;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using zResourcePrecacher7GC.Config;
using zResourcePrecacher7GC.Reporting;

public sealed class DiscordWebhookService : IDiscordWebhookService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private readonly IPrecacheReporter reporter;
    private readonly ILogger<DiscordWebhookService> logger;

    public DiscordWebhookService(IPrecacheReporter reporter, ILogger<DiscordWebhookService> logger)
    {
        this.reporter = reporter;
        this.logger = logger;
    }

    public async Task DispatchReportAsync(PrecacheReport report, DiscordConfig config, CancellationToken cancellationToken = default)
    {
        if (config.SaveLocalLogFile)
        {
            try
            {
                string? savedPath = this.reporter.SaveReport(report, config.LocalLogDirectory);
                if (savedPath != null)
                {
                    this.logger.LogInformation("Saved local precache report to '{Path}'", savedPath);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to save local precache report: {Message}", ex.Message);
            }
        }

        if (!config.Enabled || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            return;
        }

        try
        {
            int color = ParseColorHex(config.EmbedColorHex, report.FailedCount > 0);

            var embed = new Dictionary<string, object?>
            {
                ["title"] = report.FailedCount == 0 ? "✅ Precache Completed" : "⚠️ Precache Completed with Errors",
                ["description"] = "Precached resources have been successfully evaluated and registered for Counter-Strike 2.",
                ["color"] = color,
                ["timestamp"] = report.Timestamp.ToString("o", CultureInfo.InvariantCulture),
                ["footer"] = new Dictionary<string, string>
                {
                    ["text"] = "zResourcePrecacher7GC • SwiftlyS2"
                },
                ["fields"] = new List<object>
                {
                    new { name = "🗺️ Map", value = $"`{report.MapName}`", @inline = true },
                    new { name = "⏱️ Duration", value = $"`{report.ElapsedMilliseconds} ms`", @inline = true },
                    new { name = "📦 Total Items", value = $"`{report.TotalCount}`", @inline = true },
                    new { name = "✅ Precached", value = $"`{report.PassedCount} Passed`", @inline = true },
                    new { name = "⚠️ Warnings", value = $"`{report.WarningCount} Skipped`", @inline = true },
                    new { name = "📁 Packages", value = $"`{report.VpkPackages.Count} Scanned`", @inline = true }
                }
            };

            var payloadObject = new Dictionary<string, object?>
            {
                ["username"] = string.IsNullOrWhiteSpace(config.BotName) ? "zResourcePrecacher7GC" : config.BotName,
                ["embeds"] = new[] { embed }
            };

            if (!string.IsNullOrWhiteSpace(config.AvatarUrl))
            {
                payloadObject["avatar_url"] = config.AvatarUrl;
            }

            string payloadJson = JsonSerializer.Serialize(payloadObject);
            string safeMapName = string.Join("_", report.MapName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"precache_{safeMapName}_{report.Timestamp:yyyyMMdd_HHmmss}.txt";
            string logText = this.reporter.FormatReportText(report);

            using var request = new HttpRequestMessage(HttpMethod.Post, config.WebhookUrl);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("zResourcePrecacher7GC", "1.0.0"));

            if (config.AttachLogFile)
            {
                var multipartContent = new MultipartFormDataContent();

                var jsonContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");
                multipartContent.Add(jsonContent, "payload_json");

                var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(logText));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                multipartContent.Add(fileContent, "files[0]", fileName);

                request.Content = multipartContent;
            }
            else
            {
                request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                this.logger.LogInformation("Successfully dispatched precache report to Discord webhook.");
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                this.logger.LogWarning("Discord webhook responded with HTTP {Code}: {Body}", (int)response.StatusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to send precache report to Discord webhook: {Message}", ex.Message);
        }
    }

    private static int ParseColorHex(string hex, bool hasErrors)
    {
        if (hasErrors)
        {
            return 0xE74C3C;
        }

        if (string.IsNullOrWhiteSpace(hex))
        {
            return 0x2ECC71;
        }

        string clean = hex.Trim().TrimStart('#');
        if (int.TryParse(clean, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }

        return 0x2ECC71;
    }
}
