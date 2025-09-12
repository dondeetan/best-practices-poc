using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using StackExchange.Redis;
using Api.Interfaces;
using Api.Services;

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
builder.Services.AddHttpClient();

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

// Redis connection (Azure Redis Cache)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    // Example Azure Redis connection string pattern:
    // "<your-redis-name>.redis.cache.windows.net:6380,password=<primary-key>,ssl=True,abortConnect=False"
    var connStr = builder.Configuration.GetConnectionString("Redis") 
                  ?? throw new InvalidOperationException("Missing Redis connection string");
    return ConnectionMultiplexer.Connect(connStr);
});

builder.Services.AddScoped<ICarCache, CarCacheService>();
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
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];
var signingKey = builder.Configuration["Jwt:SigningKey"];

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
