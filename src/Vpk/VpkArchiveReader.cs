namespace zResourcePrecacher7GC.Vpk;

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using ValvePak;

public sealed class VpkArchiveReader : IVpkArchiveReader
{
    private readonly ILogger<VpkArchiveReader> logger;

    public VpkArchiveReader(ILogger<VpkArchiveReader> logger)
    {
        this.logger = logger;
    }

    public IReadOnlyList<string> ReadResourcePaths(string vpkFilePath, ISet<string> allowedExtensions)
    {
        var results = new List<string>();

        if (!File.Exists(vpkFilePath))
        {
            this.logger.LogWarning("VPK file does not exist: '{Path}'", vpkFilePath);
            return results;
        }

        try
        {
            using var package = new Package();
            package.Read(vpkFilePath);

            if (package.Entries == null)
            {
                return results;
            }

            foreach (var fileType in package.Entries)
            {
                string extension = fileType.Key;
                string baseExtension = extension.EndsWith("_c", StringComparison.OrdinalIgnoreCase)
                    ? extension[..^2]
                    : extension;

                if (!allowedExtensions.Contains(extension) && !allowedExtensions.Contains(baseExtension))
                {
                    continue;
                }

                foreach (PackageEntry entry in fileType.Value)
                {
                    string fullPath = entry.GetFullPath();

                    if (fullPath.EndsWith("_c", StringComparison.OrdinalIgnoreCase))
                    {
                        fullPath = fullPath[..^2];
                    }

                    string normalized = fullPath.Replace('\\', '/').Trim().TrimStart('/');
                    results.Add(normalized);
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to read VPK package '{Path}': {Message}", vpkFilePath, ex.Message);
        }

        return results;
    }
}
