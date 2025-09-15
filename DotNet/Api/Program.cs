using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Api.Interfaces;
using Api.Services;
using System.Text;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Load configuration files
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add webhost services to the container 
builder.WebHost.UseUrls("http://0.0.0.0:8085");

// Add services to the container.
builder.Services.AddControllers();

// Register HttpClient for DI
builder.Services.AddHttpClient("cars-api", (provider, client) =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var baseUrl = config["CarsApiBaseUrl"] ?? throw new InvalidOperationException("CarsApi:BaseUrl not set");
    client.BaseAddress = new Uri(baseUrl);
    var apiKey = config["CarsApiKey"];
    var apiUser = config["CarsApiUser"];
    if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiUser))
    {
        var basicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUser}:{apiKey}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);
    }
});

// Add authentication services
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]))
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// System.Text.Json options (camelCase + enums as strings if needed)
builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Use CarApiService instead of CarCacheService
builder.Services.AddScoped<ICarCache, CarApiService>();
builder.Services.AddSingleton<IEmployeeRepository, InMemoryEmployeeRepositoryService>();

// Configure the HTTP request pipeline.
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Example usage of JwtTokenValidator
var jwtToken = "your-jwt-token"; // Replace with the actual token
var issuer = builder.Configuration["Jwt:Issuer"] ?? string.Empty;
var audience = builder.Configuration["Jwt:Audience"] ?? string.Empty;
var signingKey = builder.Configuration["Jwt:SigningKey"] ?? string.Empty;

var claimsPrincipal = Api.Helpers.JwtTokenValidator.ValidateToken(jwtToken, issuer, audience, signingKey);

if (claimsPrincipal == null)
{
    Console.WriteLine("Invalid token");
}
else
{
    Console.WriteLine("Valid token");
}

app.Run();
