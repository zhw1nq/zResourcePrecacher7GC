namespace zResourcePrecacher7GC.Reporting;

public interface IPrecacheReporter
{
    string FormatReportText(PrecacheReport report);
    string? SaveReport(PrecacheReport report, string relativeDirectory);
}
