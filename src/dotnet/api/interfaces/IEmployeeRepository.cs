using Api.Entities;

namespace Api.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetAsync(int id);
    Task<Employee> CreateAsync(Employee employee);
    Task<bool> UpdateAsync(int id, Employee employee);
    Task<bool> DeleteAsync(int id);
}