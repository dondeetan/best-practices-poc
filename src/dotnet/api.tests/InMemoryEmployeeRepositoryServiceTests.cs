using Api.Entities;
using Api.Services;
using Xunit;

namespace Api.Tests;

public class InMemoryEmployeeRepositoryServiceTests
{
    [Fact]
    public async Task Constructor_SeedsFiveEmployees()
    {
        var repo = new InMemoryEmployeeRepositoryService();

        var existing = await repo.GetAsync(1);
        var missing = await repo.GetAsync(9999);

        Assert.NotNull(existing);
        Assert.Equal(1, existing!.Id);
        Assert.Null(missing);
    }

    [Fact]
    public async Task CreateAsync_WithIdZero_AssignsNextId()
    {
        var repo = new InMemoryEmployeeRepositoryService();
        var input = new Employee
        {
            Id = 0,
            FirstName = "New",
            LastName = "Employee",
            Email = "new@example.com",
            Vehicles = []
        };

        var created = await repo.CreateAsync(input);

        Assert.True(created.Id > 0);
        var fromStore = await repo.GetAsync(created.Id);
        Assert.NotNull(fromStore);
        Assert.Equal("new@example.com", fromStore!.Email);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeMissing_ReturnsFalse()
    {
        var repo = new InMemoryEmployeeRepositoryService();
        var input = new Employee
        {
            Id = 10,
            FirstName = "Ghost",
            LastName = "User",
            Email = "ghost@example.com",
            Vehicles = []
        };

        var updated = await repo.UpdateAsync(9999, input);

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeExists_RemovesEmployee()
    {
        var repo = new InMemoryEmployeeRepositoryService();

        var deleted = await repo.DeleteAsync(1);
        var employee = await repo.GetAsync(1);

        Assert.True(deleted);
        Assert.Null(employee);
    }
}
