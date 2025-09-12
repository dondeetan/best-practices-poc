using Api.Entities;

namespace Api.Interfaces;

public interface ICarCache
{
    Task<List<Car>> GetVehiclesForEmployeeAsync(int employeeId);
    Task SetVehiclesForEmployeeAsync(int employeeId, List<Car> vehicles);
    Task DeleteVehiclesForEmployeeAsync(int employeeId);
}