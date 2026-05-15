using Api.Entities;

namespace Api.Interfaces;

// Interface Segregation Principle (SOLID): controllers that only read vehicles do not depend on cache mutation operations.
public interface IEmployeeVehicleReader
{
    Task<List<Car>> GetVehiclesForEmployeeAsync(int employeeId);
}

// Interface Segregation Principle (SOLID): write operations are isolated so read-only adapters can remain substitutable.
public interface IEmployeeVehicleWriter
{
    Task SetVehiclesForEmployeeAsync(int employeeId, List<Car> vehicles);
    Task DeleteVehiclesForEmployeeAsync(int employeeId);
}

// Facade pattern: preserves the original combined cache contract for implementations that support the full vehicle cache workflow.
public interface ICarCache : IEmployeeVehicleReader, IEmployeeVehicleWriter
{
}
