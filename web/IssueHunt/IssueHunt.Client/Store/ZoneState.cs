using Fluxor;
using IssueHunt.Client.Models;
using IssueHunt.Client.Services;

namespace IssueHunt.Client.Store;

public enum ZoneLoadingState
{
    Initializing,
    Loading,
    Ready
}

public record ZoneTrackFilter(int? SiteId, string? Brand, string? Model, string? Sku);

public record ZoneState(
    ZoneLoadingState LoadingState, 
    int? ZoneId, 
    Zone? Zone, 
    ZoneTrackFilter? ZoneTrackFilter,
    DateTime PlayAt
    );

public class ZoneStateFeature : Feature<ZoneState>
{
    public override string GetName() => "ZoneState";
    protected override ZoneState GetInitialState() => new ZoneState(
        ZoneLoadingState.Initializing, 
        ZoneId: null, 
        Zone: null, 
        ZoneTrackFilter: null,
        PlayAt: DateTime.Now);
}

public record LoadZoneAction(int ZoneId, bool AutoLoadPriorityTrack);
public record LoadZoneSuccessAction(Zone Zone, bool AutoLoadPriorityTrack);
public record LoadZoneErrorAction;
public record SetZoneFilterAction(ZoneTrackFilter Filter);
public record UpdatePlayAtAction(DateTime PlayAt);

public static class ZoneReducers
{
    [ReducerMethod]
    public static ZoneState LoadZoneAction(ZoneState state, LoadZoneAction action) =>
        state with { ZoneId = action.ZoneId, LoadingState = ZoneLoadingState.Loading };
    
    [ReducerMethod]
    public static ZoneState LoadZoneSuccessAction(ZoneState state, LoadZoneSuccessAction action) =>
        state with { Zone = action.Zone, LoadingState = ZoneLoadingState.Ready };
    
    [ReducerMethod]
    public static ZoneState SetZoneFilterAction(ZoneState state, SetZoneFilterAction action) =>
        state with { ZoneTrackFilter = action.Filter };
    
    [ReducerMethod]
    public static ZoneState UpdatePlayAtAction(ZoneState state, UpdatePlayAtAction action) =>
        state with { PlayAt = action.PlayAt };
}

public class LoadZoneEffect(IZoneService zoneService) : Effect<LoadZoneAction>
{
    public override async Task HandleAsync(LoadZoneAction action, IDispatcher dispatcher)
    {
        var zone = await zoneService.GetZone(action.ZoneId);
        if (zone is not null) dispatcher.Dispatch(new LoadZoneSuccessAction(zone, action.AutoLoadPriorityTrack));
        else dispatcher.Dispatch(new LoadZoneErrorAction());
    }
}

public class LoadZoneSuccessEffect(IState<ZoneState> zoneState) : Effect<LoadZoneSuccessAction>
{
    public override Task HandleAsync(LoadZoneSuccessAction action, IDispatcher dispatcher)
    {
        if (zoneState.Value.Zone is null) dispatcher.Dispatch(new LoadZoneErrorAction());

        if (action.AutoLoadPriorityTrack)
        {
            var track = zoneState.Value.Zone?.GetPriorityTrack(
                zoneState.Value.ZoneTrackFilter?.SiteId,
                zoneState.Value.ZoneTrackFilter?.Brand,
                zoneState.Value.ZoneTrackFilter?.Model,
                zoneState.Value.ZoneTrackFilter?.Sku,
                zoneState.Value.PlayAt
            );
        
            if (track is not null) dispatcher.Dispatch(new TrackChangeRequest(track, zoneState.Value.PlayAt));
        }

        return Task.CompletedTask;
    }
}
