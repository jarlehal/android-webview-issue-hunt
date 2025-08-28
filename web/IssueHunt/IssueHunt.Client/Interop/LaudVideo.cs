using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IssueHunt.Client.Interop;

public class LaudVideo : IAsyncDisposable
{
    private const string IdbDatabaseName = "Laud-Videos";
    private const string IdbStoreName = "VideoStore";
    private const int IdbVersion = 1;
    
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private IJSObjectReference? _laudVideo;

    public LaudVideo(IJSRuntime jsRuntime)
    {
        _moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/video.js").AsTask());
    }
    
    public async Task Initialize()
    {
        var module = await _moduleTask.Value;
        _laudVideo = await module.InvokeAsync<IJSObjectReference>("createLaudVideo", IdbDatabaseName, IdbStoreName, IdbVersion);
        
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        await _laudVideo.InvokeVoidAsync("initialize");
    }
    
    public async Task DownloadIfNotAlreadyInStore(string url)
    {
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        await _laudVideo.InvokeVoidAsync("downloadIfNotAlreadyInStore", url);
    }

    public async Task<string> GetDownloadedUrls()
    {
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        return await _laudVideo.InvokeAsync<string>("getDownloadedUrls");
    }

    public async Task DeleteVideo(string id)
    {
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        await _laudVideo.InvokeVoidAsync("deleteVideo", id);
    }

    public async Task<double> GetVideoDuration(string elementId)
    {
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        return await _laudVideo.InvokeAsync<double>("getVideoDuration", elementId);
    }
    
    public async Task<double> GetIdbVideoDuration(string url)
    {
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        return await _laudVideo.InvokeAsync<double>("getIdbVideoDuration", url);
    }
    
    public async Task<string> GenerateIdbThumbnail(string url, double seek)
    {
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        return await _laudVideo.InvokeAsync<string>("generateIdbThumbnail", url, seek);
    }
    
    public async Task SetVideoSource(string elementId, string url, bool mute)
    {
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        await _laudVideo.InvokeVoidAsync("setVideoSource", elementId, url, mute);
    }
    
    public async Task SetThumbnailSource(ElementReference element, string url)
    {
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        await _laudVideo.InvokeVoidAsync("setThumbnailSource", element, url);
    }
    
    public async Task PlayVideo(string elementId, double seek)
    {
        if (_laudVideo == null) throw new InvalidOperationException("LaudVideo instance not created.");
        await _laudVideo.InvokeVoidAsync("playVideo", elementId, seek);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}