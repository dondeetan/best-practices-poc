using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Api.Entities;
using Api.Interfaces;
using System.Net.Http.Headers;

namespace Api.Services;

public class CarApiService : ICarCache
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CarApiService> _logger;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public CarApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CarApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<Car>> GetVehiclesForEmployeeAsync(int employeeId)
    {
        var clientWithBasicAuth = _httpClientFactory.CreateClient("cars-api");

        // Get Token
        var tokenresponse = await clientWithBasicAuth.PostAsync("auth/token", null);
        if (!tokenresponse.IsSuccessStatusCode)
        {
            _logger.LogError("Cars API returned {status}", tokenresponse.StatusCode);
            return [];
        }

        var json = await tokenresponse.Content.ReadAsStringAsync();

        var token = JsonSerializer.Deserialize<Token>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new Token();

        var clientWithTokenAuth = _httpClientFactory.CreateClient();
        clientWithTokenAuth.BaseAddress = clientWithBasicAuth.BaseAddress;
        clientWithTokenAuth.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Adjust to your endpoint path (e.g., /api/cars)
        var response = await clientWithTokenAuth.GetAsync("api/cars");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cars API returned {status}", response.StatusCode);
            return [];
        }

        var jsonresponse = await response.Content.ReadAsStringAsync();

        var cars = JsonSerializer.Deserialize<List<Car>>(jsonresponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Car>();
        return cars;
    }

    public Task SetVehiclesForEmployeeAsync(int employeeId, List<Car> vehicles)
    {
        throw new NotImplementedException("Setting vehicles is not supported via API");
    }

    public Task DeleteVehiclesForEmployeeAsync(int employeeId)
    {
        throw new NotImplementedException("Deleting vehicles is not supported via API");
    }
}
