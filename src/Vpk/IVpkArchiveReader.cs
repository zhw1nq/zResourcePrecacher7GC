namespace zResourcePrecacher7GC.Vpk;

using System.Collections.Generic;

public interface IVpkArchiveReader
{
    IReadOnlyList<string> ReadResourcePaths(string vpkFilePath, ISet<string> allowedExtensions);
}
