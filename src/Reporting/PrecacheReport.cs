namespace zResourcePrecacher7GC.Reporting;

using System;
using System.Collections.Generic;

public enum PrecacheItemStatus
{
    Pass,
    Warn,
    Fail
}

public sealed class PrecacheReportItem
{
    public string Path { get; }
    public PrecacheItemStatus Status { get; }
    public string? Reason { get; }

    public PrecacheReportItem(string path, PrecacheItemStatus status, string? reason = null)
    {
        this.Path = path;
        this.Status = status;
        this.Reason = reason;
    }
}

public sealed class PrecacheVpkSummary
{
    public string PackageName { get; set; } = string.Empty;
    public int EntryCount { get; set; }
}

public sealed class PrecacheReport
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string ServerHostname { get; set; } = "CS2 Server";
    public string MapName { get; set; } = "unknown";
    public long ElapsedMilliseconds { get; set; }
    public List<PrecacheVpkSummary> VpkPackages { get; } = new();
    public List<PrecacheReportItem> Items { get; } = new();

    public int PassedCount => this.Items.FindAll(x => x.Status == PrecacheItemStatus.Pass).Count;
    public int WarningCount => this.Items.FindAll(x => x.Status == PrecacheItemStatus.Warn).Count;
    public int FailedCount => this.Items.FindAll(x => x.Status == PrecacheItemStatus.Fail).Count;
    public int TotalCount => this.Items.Count;
}
