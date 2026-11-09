namespace zResourcePrecacher7GC.Events;

using SwiftlyS2.Shared.Events;
using zResourcePrecacher7GC.Services;

public sealed class PrecacheEventListener
{
    private readonly IPrecacheService precacheService;

    public PrecacheEventListener(IPrecacheService precacheService)
    {
        this.precacheService = precacheService;
    }

    [EventListener<EventDelegates.OnPrecacheResource>]
    public void OnPrecacheResource(IOnPrecacheResourceEvent @event)
    {
        this.precacheService.ExecutePrecache(@event);
    }

    [EventListener<EventDelegates.OnMapLoad>]
    public void OnMapLoad(IOnMapLoadEvent @event)
    {
        this.precacheService.SetCurrentMap(@event.MapName);
    }
}
