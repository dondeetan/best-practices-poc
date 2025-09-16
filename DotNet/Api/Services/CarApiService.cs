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
        var client = _httpClientFactory.CreateClient("cars-api");
        try
        {
            var accessToken = await GetApiTokenAsync(client);
            if (string.IsNullOrWhiteSpace(accessToken)) return new List<Car>();

            var apiClient = _httpClientFactory.CreateClient();
            apiClient.BaseAddress = client.BaseAddress;
            return await GetCarsForEmployeeAsync(apiClient, employeeId, accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while fetching vehicles for employee {employeeId}", employeeId);
            return new List<Car>();
        }
    }

    public Task SetVehiclesForEmployeeAsync(int employeeId, List<Car> vehicles)
    {
        throw new NotImplementedException("Setting vehicles is not supported via API");
    }

    public Task DeleteVehiclesForEmployeeAsync(int employeeId)
    {
        throw new NotImplementedException("Deleting vehicles is not supported via API");
    }

    
    private async Task<string?> GetApiTokenAsync(HttpClient client)
    {
        var tokenResponse = await client.PostAsync("auth/token", null);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Cars API token request failed: {status}", tokenResponse.StatusCode);
            return null;
        }
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<Token>(tokenJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            _logger.LogError("Cars API token is null or empty");
            return null;
        }
        return token.AccessToken;
    }

    private async Task<List<Car>> GetCarsForEmployeeAsync(HttpClient apiClient, int employeeId, string accessToken)
    {
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await apiClient.GetAsync($"api/cars?employeeid={employeeId}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cars API returned {status} for employee {employeeId}", response.StatusCode, employeeId);
            return new List<Car>();
        }
        var carsJson = await response.Content.ReadAsStringAsync();
        var cars = JsonSerializer.Deserialize<List<Car>>(carsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Car>();
        return cars;
    }
}
