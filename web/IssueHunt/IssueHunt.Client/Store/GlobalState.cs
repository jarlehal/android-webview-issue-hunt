using Fluxor;

namespace IssueHunt.Client.Store;

public enum ReadyStateEnum
{
    Initialized,
    Loading,
    Calculating,
    Ready
}

public enum LiveVideoModeEnum
{
    FullScreenZoomed,
    FullScreenAspectRatio
}

public enum ActivityStateEnum
{
    Active,
    Inactive
}

public record ProgressInfo(int? Progress, string? Title, string? Subtitle, string? Message);

public record GlobalState(LiveVideoModeEnum LiveVideoMode, ActivityStateEnum ActivityState, ReadyStateEnum ReadyState, ProgressInfo? ProgressInfo = null);

public record SetActive;
public record SetInactive;
public record SetLiveVideoMode(LiveVideoModeEnum LiveVideoMode);

public class GlobalStateFeature : Feature<GlobalState>
{
    public override string GetName() => "GlobalState";
    protected override GlobalState GetInitialState() => new GlobalState(
        LiveVideoModeEnum.FullScreenZoomed, ActivityStateEnum.Active, 
        ReadyStateEnum.Initialized, null);
}

public class GlobalStateReducers
{
    [ReducerMethod]
    public static GlobalState ReduceSetActiveAction(GlobalState state, SetActive action) =>
        state with { ActivityState = ActivityStateEnum.Active };
    
    [ReducerMethod]
    public static GlobalState ReduceSetInactiveAction(GlobalState state, SetInactive action) =>
        state with { ActivityState = ActivityStateEnum.Inactive };
    
    [ReducerMethod]
    public static GlobalState ReduceSetFullScreen(GlobalState state, SetLiveVideoMode action) =>
        state with { LiveVideoMode = action.LiveVideoMode };
    
}