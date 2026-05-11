using System.Net;
using System.Net.Http.Headers;
using Functions;
using Functions.Entities;
using Functions.Interfaces;
using Functions.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Functions.Tests;

public class CarsSyncTests
{
    [Fact]
    public async Task CarsApiClient_WhenTokenRequestFails_DoesNotRequestCars()
    {
        var tokenHandler = new RecordingHandler(
            new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var carsHandler = new RecordingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });

        var client = new CarsApiClient(
            new StubHttpClientFactory(
                CreateClient(tokenHandler),
                CreateClient(carsHandler)),
            NullLogger<CarsApiClient>.Instance);

        var cars = await client.GetCarsAsync(CancellationToken.None);

        Assert.Empty(cars);
        Assert.Single(tokenHandler.Requests);
        Assert.Empty(carsHandler.Requests);
        Assert.Equal("https://cars.example/auth/token", tokenHandler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task CarsApiClient_WhenTokenSucceeds_FetchesCarsWithBearerToken()
    {
        var tokenHandler = new RecordingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"access_token":"token-value","token_type":"bearer","expires_in_minutes":60}""")
            });
        var carsHandler = new RecordingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """[{"id":1,"size":"m","fuel":"hybrid","doors":4,"transmission":"auto","trips":[],"employeeid":0}]""")
            });

        var client = new CarsApiClient(
            new StubHttpClientFactory(
                CreateClient(tokenHandler),
                CreateClient(carsHandler)),
            NullLogger<CarsApiClient>.Instance);

        var cars = await client.GetCarsAsync(CancellationToken.None);

        Assert.Single(cars);
        Assert.Single(tokenHandler.Requests);
        Assert.Single(carsHandler.Requests);
        Assert.Equal("https://cars.example/api/cars", carsHandler.Requests[0].RequestUri!.ToString());
        Assert.Equal("Bearer", carsHandler.Requests[0].Headers.Authorization?.Scheme);
        Assert.Equal("token-value", carsHandler.Requests[0].Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task RunAsync_DelegatesCarsToCacheWriter()
    {
        var cars = new List<Car>
        {
            new() { Id = 1, EmployeeId = 12, Fuel = "hybrid" }
        };
        var apiClient = new StubCarsApiClient(cars);
        var cacheWriter = new RecordingCarsCacheWriter();
        var function = new CarsSync(apiClient, cacheWriter, NullLogger<CarsSync>.Instance);

        await function.RunAsync(null!, CancellationToken.None);

        Assert.Same(cars, cacheWriter.Cars);
    }

    private static HttpClient CreateClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://cars.example/")
        };
    }

    private sealed class StubHttpClientFactory(HttpClient namedClient, HttpClient defaultClient) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return string.Equals(name, "cars-api", StringComparison.Ordinal)
                ? namedClient
                : defaultClient;
        }
    }

    private sealed class RecordingHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(Clone(request));
            return Task.FromResult(_responses.Dequeue());
        }

        private static HttpRequestMessage Clone(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content is not null)
            {
                clone.Content = new StringContent(request.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            clone.Headers.Authorization = request.Headers.Authorization is null
                ? null
                : new AuthenticationHeaderValue(
                    request.Headers.Authorization.Scheme,
                    request.Headers.Authorization.Parameter);

            return clone;
        }
    }

    private sealed class StubCarsApiClient(IReadOnlyCollection<Car> cars) : ICarsApiClient
    {
        public Task<IReadOnlyCollection<Car>> GetCarsAsync(CancellationToken cancellationToken) => Task.FromResult(cars);
    }

    private sealed class RecordingCarsCacheWriter : ICarsCacheWriter
    {
        public IReadOnlyCollection<Car>? Cars { get; private set; }

        public Task WriteAsync(IReadOnlyCollection<Car> cars, CancellationToken cancellationToken)
        {
            Cars = cars;
            return Task.CompletedTask;
        }
    }
}

