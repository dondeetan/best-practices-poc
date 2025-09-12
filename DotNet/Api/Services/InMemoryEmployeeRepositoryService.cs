

using Api.Entities;
using Api.Interfaces;

namespace Api.Services;

public class InMemoryEmployeeRepositoryService : IEmployeeRepository
{
    private readonly Dictionary<int, Employee> _store = new();

    public InMemoryEmployeeRepositoryService() {}
    public Task<Employee?> GetAsync(int id)
    {
        _store.TryGetValue(id, out var e);
        return Task.FromResult(e);
    }

    public Task<Employee?> CreateAsync(Employee employee)
    {
        if (employee.Id == 0)
        {
            employee.Id = _store.Count == 0 ? 1 : _store.Keys.Max() + 1;            
        }
        _store[employee.Id] = employee;
        return Task.FromResult(employee);
    }

    public Task<bool> UpdateAsync(int id, Employee employee)
    {
        if (!_store.ContainsKey(id)) return Task.FromResult(false);
        employee.Id = id;
        _store[id] = employee;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var ok = _store.Remove(id);
        return Task.FromResult(ok);
    }
}