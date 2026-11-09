namespace zResourcePrecacher7GC.Services;

using SwiftlyS2.Shared.Events;

public interface IPrecacheService
{
    int PreparedResourceCount { get; }
    int ScannedPackageCount { get; }
    string CurrentMapName { get; }

    void Reload();
    void SetCurrentMap(string mapName);
    void ExecutePrecache(IOnPrecacheResourceEvent @event);
}
