using Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Functions.Tests;

public class HttpTriggerTests
{
    [Fact]
    public void GetAuthorizationLevelAnonymous_ReturnsAnonymousWelcomeMessage()
    {
        var logger = new Mock<ILogger<HttpTrigger>>();
        var function = new HttpTrigger(logger.Object);

        var result = function.GetAuthorizationLevelAnonymous(new DefaultHttpContext().Request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Anonymous - Welcome to Azure Functions!", ok.Value);
    }

    [Fact]
    public void GetAuthorizationLevelFunction_ReturnsFunctionWelcomeMessage()
    {
        var logger = new Mock<ILogger<HttpTrigger>>();
        var function = new HttpTrigger(logger.Object);

        var result = function.GetAuthorizationLevelFunction(new DefaultHttpContext().Request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Function - Welcome to Azure Functions!", ok.Value);
    }
}
