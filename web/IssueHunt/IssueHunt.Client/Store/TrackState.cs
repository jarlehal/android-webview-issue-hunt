using Fluxor;
using IssueHunt.Client.Models;

namespace IssueHunt.Client.Store;

public enum TrackLoadingState
{
    Uninitialized,
    Requested,
    Downloading,
    DownloadCompleted,
    Ready
}

[FeatureState]
public record TrackState(
    TrackLoadingState LoadingState, 
    Playlist? CurrentPlaylist, 
    PlaylistItem? CurrentPlaylistItem,
    DateTime PlayAt,
    Track? CurrentTrack, 
    Track? RequestedTrack, 
    bool ShowTrackDetails,
    PlaylistItem? PreviewPlaylistItem
    )
{
    public static readonly TrackState Empty = new();
    private TrackState() : this
    (
        LoadingState: TrackLoadingState.Uninitialized,
        CurrentPlaylist: null,
        CurrentPlaylistItem: null,
        PlayAt: DateTime.Now,
        CurrentTrack: null,
        RequestedTrack: null,
        ShowTrackDetails: false,
        PreviewPlaylistItem: null
    ) { }
}

public record TrackChangeRequest(Track? Track, DateTime PlayAt);
public record TrackChangeDownloading;

public record TrackChangeDownloadCompleted(IEnumerable<PlaylistItem> LoadedItems);
public record TrackChangeReadyRequest;
public record TrackChangeReadyResponse;

public record PlaylistChange(Playlist? Playlist);
public record PlaylistItemChange(PlaylistItem? PlaylistItem, double SeekSeconds, int PlaylistItemIndex);

public record TrackPauseRequest;
public record TrackResumeRequest;

public record ShowTrackDetails;
public record HideTrackDetails;

public record ShowPreviewPlaylistItem(PlaylistItem? PlaylistItem);
public record HidePreviewPlaylistItem;

public class TrackReducers
{
    [ReducerMethod]
    public static TrackState ReduceTrackChangeRequest(TrackState state, TrackChangeRequest action) =>
        state with { LoadingState = TrackLoadingState.Requested, RequestedTrack = action.Track, PlayAt = action.PlayAt };
    
    [ReducerMethod]
    public static TrackState ReduceTrackChangeDownloading(TrackState state, TrackChangeDownloading action) =>
        state with { LoadingState = TrackLoadingState.Downloading };
    
    [ReducerMethod]
    public static TrackState ReduceTrackChangeLoadingCompleted(TrackState state, TrackChangeDownloadCompleted action) =>
        state with { LoadingState = TrackLoadingState.DownloadCompleted };
    
    [ReducerMethod]
    public static TrackState ReduceTrackChangeReadyRequest(TrackState state, TrackChangeReadyRequest action) =>
        state with
        {
            LoadingState = TrackLoadingState.Ready, 
            CurrentTrack = state.RequestedTrack, 
            RequestedTrack = null,
            CurrentPlaylist = state.CurrentTrack?.Playlists?.Where(x => x.IsInPeriod(state.PlayAt)).FirstOrDefault()
        };

    [ReducerMethod]
    public static TrackState ReduceTrackChangeReadyResponse(TrackState state, TrackChangeReadyResponse action) =>
        state;
    
    [ReducerMethod]
    public static TrackState ReducePlaylistChange(TrackState state, PlaylistChange action) =>
        state with { CurrentPlaylist = action.Playlist };
    
    [ReducerMethod]
    public static TrackState ReducePlaylistItemChange(TrackState state, PlaylistItemChange action) =>
        state with { CurrentPlaylistItem = action.PlaylistItem };
    
    [ReducerMethod]
    public static TrackState ReduceShowTrackDetails(TrackState state, ShowTrackDetails action) =>
        state with { ShowTrackDetails = true };
    
    [ReducerMethod]
    public static TrackState ReduceHideTrackDetails(TrackState state, HideTrackDetails action) =>
        state with { ShowTrackDetails = false };
    
    [ReducerMethod]
    public static TrackState ReduceShowPreviewPlaylistItem(TrackState state, ShowPreviewPlaylistItem action) =>
        state with { PreviewPlaylistItem = action.PlaylistItem };
    
    [ReducerMethod]
    public static TrackState ReduceHidePreviewPlaylistItem(TrackState state, HidePreviewPlaylistItem action) =>
        state with { PreviewPlaylistItem = null };
}

public class TrackEffects(IState<TrackState> state)
{
    [EffectMethod]
    public Task HandleTrackChangeReadyRequest(TrackChangeReadyRequest action, IDispatcher dispatcher)
    {
        if (state.Value.LoadingState == TrackLoadingState.Ready)
            dispatcher.Dispatch(new TrackChangeReadyResponse());
        
        return Task.CompletedTask;
    }
    
    [EffectMethod]
    public Task HandleTrackChangeReady(TrackChangeReadyResponse action, IDispatcher dispatcher)
    {
        return Task.CompletedTask;
    }
    
}