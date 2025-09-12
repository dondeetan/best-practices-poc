using System.Text.Json.Serialization;

namespace Functions.Entities;

public class Car
{
    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("fuel")]
    public string? Fuel { get; set; }

    [JsonPropertyName("doors")]
    public int Doors { get; set; }

    [JsonPropertyName("transmission")]
    public string? Transmission { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("trips")]
    public List<Trip>? Trips { get; set; }

    [JsonPropertyName("employeeid")]
    public int EmployeeId { get; set; }
}