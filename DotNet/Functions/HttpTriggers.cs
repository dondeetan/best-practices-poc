using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

public class HttpTrigger
{
    private readonly ILogger<HttpTrigger> _logger;

    public HttpTrigger(ILogger<HttpTrigger> logger)
    {
        _logger = logger;
    }

    [Function("HttpTriggerGetAuthorizationLevelAnonymous")]
    public IActionResult GetAuthorizationLevelAnonymous([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request. AuthorizationLevel.Anonymous");
        return new OkObjectResult("Anonymous - Welcome to Azure Functions!");

        //HTTP Trigger to process and cache data using Redis Cache
    }

    [Function("HttpTriggerGetAuthorizationLevelFunction")]
    public IActionResult GetFAuthorizationLevelunction([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request. Using AuthorizationLevel.Function");
        return new OkObjectResult("Function - Welcome to Azure Functions!");
    }
}