using System.Net.Http.Headers;
using System.Text.Json;
using Functions.Entities;
using Functions.Interfaces;
using Microsoft.Extensions.Logging;

namespace Functions.Services;

// Template Method pattern: token acquisition and authenticated resource retrieval form the fixed Cars API workflow.
public sealed class CarsApiClient : ICarsApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CarsApiClient> _logger;

    public CarsApiClient(IHttpClientFactory httpClientFactory, ILogger<CarsApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Car>> GetCarsAsync(CancellationToken cancellationToken)
    {
        var clientWithBasicAuth = _httpClientFactory.CreateClient("cars-api");
        var accessToken = await GetAccessTokenAsync(clientWithBasicAuth, cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return [];
        }

        var clientWithTokenAuth = _httpClientFactory.CreateClient();
        clientWithTokenAuth.BaseAddress = clientWithBasicAuth.BaseAddress;
        clientWithTokenAuth.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await clientWithTokenAuth.GetAsync("api/cars", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cars API returned {StatusCode}", response.StatusCode);
            return [];
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<List<Car>>(stream, JsonOptions, cancellationToken) ?? [];
    }

    private async Task<string?> GetAccessTokenAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var tokenResponse = await client.PostAsync("auth/token", null, cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Cars API token request returned {StatusCode}", tokenResponse.StatusCode);
            return null;
        }

        await using var stream = await tokenResponse.Content.ReadAsStreamAsync(cancellationToken);
        var token = await JsonSerializer.DeserializeAsync<Token>(stream, JsonOptions, cancellationToken);
        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            _logger.LogError("Cars API token response did not include an access token");
            return null;
        }

        return token.AccessToken;
    }
}
