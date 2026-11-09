namespace zResourcePrecacher7GC.Reporting;

using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;

public sealed class PrecacheReporter : IPrecacheReporter
{
    private readonly ISwiftlyCore core;
    private readonly ILogger<PrecacheReporter> logger;

    public PrecacheReporter(ISwiftlyCore core, ILogger<PrecacheReporter> logger)
    {
        this.core = core;
        this.logger = logger;
    }

    public string FormatReportText(PrecacheReport report)
    {
        var sb = new StringBuilder(report.Items.Count * 90 + 512);

        sb.AppendLine("================================================================================");
        sb.AppendLine("           zResourcePrecacher7GC - Resource Precache Execution Log");
        sb.AppendLine("================================================================================");
        sb.AppendLine($"Timestamp       : {report.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"Server Name     : {report.ServerHostname}");
        sb.AppendLine($"Map Name        : {report.MapName}");
        sb.AppendLine($"Elapsed Time    : {report.ElapsedMilliseconds} ms");
        sb.AppendLine($"Total Evaluated : {report.TotalCount}");
        sb.AppendLine($"Passed (Precached) : {report.PassedCount}");
        sb.AppendLine($"Warnings (Skipped) : {report.WarningCount}");
        sb.AppendLine($"Failed (Errors)    : {report.FailedCount}");
        sb.AppendLine();

        if (report.VpkPackages.Count > 0)
        {
            sb.AppendLine("Scanned VPK Packages:");
            foreach (var vpk in report.VpkPackages)
            {
                sb.AppendLine($"  - {vpk.PackageName} ({vpk.EntryCount} items)");
            }
            sb.AppendLine();
        }

        sb.AppendLine("================================================================================");
        foreach (var item in report.Items)
        {
            switch (item.Status)
            {
                case PrecacheItemStatus.Pass:
                    sb.AppendLine($"[PASS] {item.Path}");
                    break;
                case PrecacheItemStatus.Warn:
                    sb.AppendLine($"[WARN] {item.Path}{(string.IsNullOrEmpty(item.Reason) ? "" : $" ({item.Reason})")}");
                    break;
                case PrecacheItemStatus.Fail:
                    sb.AppendLine($"[FAIL] {item.Path}{(string.IsNullOrEmpty(item.Reason) ? "" : $" ({item.Reason})")}");
                    break;
            }
        }
        sb.AppendLine("================================================================================");
        sb.AppendLine("End of Precache Log.");

        return sb.ToString();
    }

    public string? SaveReport(PrecacheReport report, string relativeDirectory)
    {
        try
        {
            string baseDir = string.IsNullOrWhiteSpace(this.core.PluginPath)
                ? AppContext.BaseDirectory
                : this.core.PluginPath;

            string targetDir = Path.Combine(baseDir, relativeDirectory);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            string safeMapName = string.Join("_", report.MapName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"precache_{safeMapName}_{report.Timestamp:yyyyMMdd_HHmmss}.txt";
            string fullPath = Path.Combine(targetDir, fileName);

            string content = this.FormatReportText(report);
            File.WriteAllText(fullPath, content, Encoding.UTF8);

            return fullPath;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to save local precache report: {Message}", ex.Message);
            return null;
        }
    }
}
