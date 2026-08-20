using Api.Entities;
using Api.Interfaces;

namespace Api.Services;

// Null Object pattern: read-only deployments can safely accept write requests without throwing infrastructure exceptions.
public sealed class NoOpEmployeeVehicleWriter : IEmployeeVehicleWriter
{
    private readonly ILogger<NoOpEmployeeVehicleWriter> _logger;

    public NoOpEmployeeVehicleWriter(ILogger<NoOpEmployeeVehicleWriter> logger)
    {
        _logger = logger;
    }

    public Task SetVehiclesForEmployeeAsync(int employeeId, List<Car> vehicles)
    {
        _logger.LogInformation("Vehicle writes are disabled for employee {EmployeeId}", employeeId);
        return Task.CompletedTask;
    }

    public Task DeleteVehiclesForEmployeeAsync(int employeeId)
    {
        _logger.LogInformation("Vehicle cache delete is disabled for employee {EmployeeId}", employeeId);
        return Task.CompletedTask;
    }
}
