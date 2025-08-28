namespace IssueHunt.Client.Models;

public class Zone
{
    public string? ContentRevision { get; set; }
    public Track[]? Tracks { get; set; }
    public TrackTrigger[]? TrackTriggers { get; set; }
    
    public Track? GetPriorityTrack(int? siteId, string? brand, string? model, string? sku, DateTime? playAt)
    {
        var dateTime = playAt ?? DateTime.Now;
        
        return Tracks?
            .Where(x => x.IsValidForSite(siteId) 
              && x.IsValidForDayOfWeek(dateTime.DayOfWeek)
              && x.IsValidForTimeOfDay(dateTime)
              && x.HasPlaylistsInPeriod(dateTime)
              && x.IsValidForBrand(brand)
              && x.IsValidForModel(model)
              && x.IsValidForSkus(sku)
              && !IsTrackTrigger(x))
            .MaxBy(x => x.Priority);
    }

    public bool IsTrackTrigger(Track track) => TrackTriggers?.Any(x => x.TrackId == track.Id ) ?? false;
}

public class Track
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Priority { get; set; }
    public PlayCondition? PlayCondition { get; set; }
    public Playlist[]? Playlists { get; set; }
    public Track[]? Subtracks { get; set; }
    
    private bool HasPlaylists => Playlists?.Length > 0;
    public bool HasPlaylistsInPeriod(DateTime playAt) => HasPlaylists && (Playlists?.Any(x => x.IsInPeriod(playAt)) ?? false);
    public bool IsInterval() => PlayCondition is {Interval: > 0};
    public bool IsValidForSite(int? siteId)
    {
        if (PlayCondition?.SiteIds is null || PlayCondition.SiteIds.Length == 0) return true;
        return siteId is not null && PlayCondition.SiteIds.Contains(siteId.Value);
    }

    public bool IsValidForDayOfWeek(DayOfWeek dayOfWeek)
    {
        if (PlayCondition?.Days is null || PlayCondition.Days.Length == 0) return true;
        return PlayCondition.Days.Contains(WeekDayNumber(dayOfWeek));
    }

    public bool IsValidForTimeOfDay(DateTime currentTimeUtc)
    {
        if (PlayCondition?.Hours is null || PlayCondition.Hours.Length == 0) return true;
        return PlayCondition.Hours.Any(x => x.CurrentTimeInsideHoursStartEnd(currentTimeUtc));
    }
    
    public bool IsValidForBrand(string? brand)
    {
        if (PlayCondition?.Brand is null or "") return true;
        if (brand is null) return false;
        return PlayCondition?.Brand == brand.ToLower();
    }
    
    public bool IsValidForModel(string? model)
    {
        if (PlayCondition?.Models is null or []) return true;
        if (model is null) return false;

        foreach (var modelQualifier in PlayCondition.Models)
        {
            if (modelQualifier.StartsWith('*') && model.ToLower().EndsWith(modelQualifier.Replace("*", ""))) return true;
            if (modelQualifier.EndsWith('*') && model.ToLower().StartsWith(modelQualifier.Replace("*", ""))) return true;
            if (modelQualifier.StartsWith('*') && modelQualifier.EndsWith('*') && model.ToLower().Contains(modelQualifier.Replace("*", ""))) return true;
        }
        
        return false;
    }

    public bool IsValidForSkus(string? sku)
    {
        if (PlayCondition?.Skus is null or []) return true;
        if (sku is null) return false;
        return PlayCondition.Skus.Contains(sku);
    }

    
    public PlaylistItem[] AllValidItems(int? siteId, string? brand, string? model, string? sku, DateTime playAt) 
    {
        var playlist = Playlists?.Where(x => x.IsInPeriod(playAt)).FirstOrDefault();
        if (playlist?.Items == null) return [];

        var items = new List<PlaylistItem>();
        foreach (var item in playlist.Items)
        {
            if (item.IsSubtrack)
            {
                if (item.Subtracks == null) continue;
                foreach (var subtrackIndex in item.Subtracks)
                {
                    var track = Subtracks?[subtrackIndex];
                    if (track == null || !track.IsValidForSite(siteId)
                                      || !track.IsValidForDayOfWeek(DateTime.UtcNow.DayOfWeek)
                                      || !track.IsValidForTimeOfDay(DateTime.UtcNow)
                                      || !track.IsValidForBrand(brand)
                                      || !track.IsValidForModel(model)
                                      || !track.IsValidForSkus(sku)) continue;
                    
                    var subtrackPlaylist = track.Playlists?.Where(x => x.IsInPeriod(playAt)).FirstOrDefault();
                    if (subtrackPlaylist?.Items == null) continue;

                    items.AddRange(subtrackPlaylist.Items);
                }
            } 
            else if (item.IsVideo)
            {
                items.Add(item);
            }
        }

        return items.ToArray();
    }
    
    private int WeekDayNumber(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => 1,
        DayOfWeek.Tuesday => 2,
        DayOfWeek.Wednesday => 3,
        DayOfWeek.Thursday => 4,
        DayOfWeek.Friday => 5,
        DayOfWeek.Saturday => 6,
        DayOfWeek.Sunday => 7,
        _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
    };
}

public class PlayCondition
{
    public int? Interval { get; set; }
    public int[]? SiteIds { get; set; }

    public PlayConditionHours[]? Hours { get; set; }
    public int[]? Days { get; set; }
    public string? Brand { get; set; }
    public string[]? Models { get; set; }
    public string[]? Skus { get; set; }
    public string? OnHigherPri { get; set; }
}

public class PlayConditionHours
{
    public string? Start { get; set; }
    public string? End { get; set; }
    
    public bool CurrentTimeInsideHoursStartEnd(DateTime currentTimeUtc)
    {
        if (Start is null || End is null) return true;
        var startDate = DateTime.Parse($"{currentTimeUtc.ToString("yyyy-MM-dd")}T{Start}");
        var endDate = DateTime.Parse($"{currentTimeUtc.ToString("yyyy-MM-dd")}T{End}");
        return currentTimeUtc >= startDate && currentTimeUtc < endDate;
    }
}

public class Playlist
{
    public PlaylistPeriod? Period { get; set; }
    public PlaylistItem[]? Items { get; set; }
    public bool HasVideos => Items?.Any(x => x.IsVideo) ?? false;
    public bool IsInPeriod(DateTime currentTime)
    {
        if (Period == null) return true;
        return currentTime > Period.Start && currentTime <= Period.End;
    } 
}

public class PlaylistPeriod
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

public class PlaylistItem
{
    public string? Type { get; set; }
    public string? Url { get; set; }
    public int Volume { get; set; }
    public string? CdnUrl => Url?
        .Replace("https://laudmedia-content.s3.eu-central-1.amazonaws.com/", "https://contentcache.laud-media.com/")
        .Replace("https://lm-test-pipeline-output.s3.eu-west-1.amazonaws.com/", "https://staging-contentcache.laud-media.com/");
    public double? Duration { get; set; }
    public int[]? Subtracks { get; set; }
    public string[]? Tags { get; set; }
    public bool IsSubtrack => Type == "subtrack";
    public bool IsVideo => Type == "video";
}

public class TrackTrigger
{
    public int? TrackId { get; set; }
    public TriggerType[]? Triggers { get; set; }
}

public class TriggerType
{
    public string? Type { get; set; }
    public string[]? Arguments { get; set; }
}