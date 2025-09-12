using System.Text.Json;
using Api.Entities;
using Api.Interfaces;
using StackExchange.Redis;

namespace Api.Services;

public class CarCacheService : ICarCache
{
    private readonly IConnectionMultiplexer _mux;
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public CarCacheService(IConnectionMultiplexer mux)
    {
        _mux = mux;
        _db = _mux.GetDatabase();
    }

    private static string Key(int employeeId) => $"employee:{employeeId}:vehicles";

    public async Task<List<Car>> GetVehiclesForEmployeeAsync(int employeeId)
    {
        var json = await _db.StringGetAsync(Key(employeeId));
        if (json.IsNullOrEmpty) return new List<Car>();
        return JsonSerializer.Deserialize<List<Car>>(json!, _json) ?? new List<Car>();
    }

    public async Task SetVehiclesForEmployeeAsync(int employeeId, List<Car> vehicles)
    {
        var json = JsonSerializer.Serialize(vehicles ?? new List<Car>(), _json);
        // Optionally set TTL: await _db.StringSetAsync(Key(employeeId), json, TimeSpan.FromHours(6));
        await _db.StringSetAsync(Key(employeeId), json);
    }

    public async Task DeleteVehiclesForEmployeeAsync(int employeeId)
    {
        await _db.KeyDeleteAsync(Key(employeeId));
    }
}