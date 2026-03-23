using System.Net;
using System.Net.Http.Headers;
using Functions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Functions.Tests;

public class CarsSyncTests
{
    [Fact]
    public async Task RunAsync_WhenTokenRequestFails_DoesNotRequestCars()
    {
        var tokenHandler = new RecordingHandler(
            new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var carsHandler = new RecordingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });

        var function = new CarsSync(
            new StubHttpClientFactory(
                CreateClient(tokenHandler),
                CreateClient(carsHandler)),
            NullLogger<CarsSync>.Instance,
            CreateConfiguration(useRedisCache: false));

        await function.RunAsync(null!);

        Assert.Single(tokenHandler.Requests);
        Assert.Empty(carsHandler.Requests);
        Assert.Equal("https://cars.example/auth/token", tokenHandler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task RunAsync_WhenRedisDisabled_FetchesCarsWithBearerToken()
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

        var function = new CarsSync(
            new StubHttpClientFactory(
                CreateClient(tokenHandler),
                CreateClient(carsHandler)),
            NullLogger<CarsSync>.Instance,
            CreateConfiguration(useRedisCache: false));

        await function.RunAsync(null!);

        Assert.Single(tokenHandler.Requests);
        Assert.Single(carsHandler.Requests);
        Assert.Equal("https://cars.example/api/cars", carsHandler.Requests[0].RequestUri!.ToString());
        Assert.Equal("Bearer", carsHandler.Requests[0].Headers.Authorization?.Scheme);
        Assert.Equal("token-value", carsHandler.Requests[0].Headers.Authorization?.Parameter);
    }

    private static HttpClient CreateClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://cars.example/")
        };
    }

    private static IConfiguration CreateConfiguration(bool useRedisCache)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseRedisCache"] = useRedisCache.ToString()
            })
            .Build();
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
}

