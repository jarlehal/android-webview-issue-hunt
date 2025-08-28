using System.Net.Http.Json;
using IssueHunt.Client.Models;

namespace IssueHunt.Client.Services;

public interface IZoneService
{
    public Task<Zone?> GetZone(int zoneId);
}

public class ZoneService(HttpClient http) : IZoneService
{
    public async Task<Zone?> GetZone(int zoneId)
        => await http.GetFromJsonAsync<Zone?>($"data/zone-{zoneId}.json");
        
}