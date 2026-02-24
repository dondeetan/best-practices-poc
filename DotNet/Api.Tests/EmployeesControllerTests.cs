using Api.Entities;
using Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Tests;

public class EmployeesControllerTests
{
    [Fact]
    public async Task GetById_WhenEmployeeMissing_ReturnsNotFound()
    {
        var repo = new Mock<IEmployeeRepository>();
        var cache = new Mock<ICarCache>();
        repo.Setup(x => x.GetAsync(42)).ReturnsAsync((Employee?)null);

        var controller = new EmployeesController(repo.Object, cache.Object);
        var result = await controller.GetById(42, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_WhenEmployeeExists_ReturnsEmployeeWithVehicles()
    {
        var repo = new Mock<IEmployeeRepository>();
        var cache = new Mock<ICarCache>();
        var employee = new Employee { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", Vehicles = [] };
        var vehicles = new List<Car> { new() { Id = 7, EmployeeId = 1, Fuel = "electric" } };

        repo.Setup(x => x.GetAsync(1)).ReturnsAsync(employee);
        cache.Setup(x => x.GetVehiclesForEmployeeAsync(1)).ReturnsAsync(vehicles);

        var controller = new EmployeesController(repo.Object, cache.Object);
        var result = await controller.GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<Employee>(ok.Value);
        Assert.Single(payload.Vehicles);
        Assert.Equal(7, payload.Vehicles[0].Id);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWithCreatedEmployee()
    {
        var repo = new Mock<IEmployeeRepository>();
        var cache = new Mock<ICarCache>();
        var input = new Employee { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Vehicles = [] };
        var created = new Employee { Id = 99, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Vehicles = [] };

        repo.Setup(x => x.CreateAsync(input)).ReturnsAsync(created);
        var controller = new EmployeesController(repo.Object, cache.Object);

        var result = await controller.Create(input, CancellationToken.None);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(EmployeesController.GetById), createdAt.ActionName);
        Assert.Equal(99, ((Employee)createdAt.Value!).Id);
    }

    [Fact]
    public async Task Update_WhenRepoUpdates_ReturnsNoContent_AndUsesRouteId()
    {
        var repo = new Mock<IEmployeeRepository>();
        var cache = new Mock<ICarCache>();
        var input = new Employee { Id = 1234, FirstName = "X", LastName = "Y", Email = "x@y.com", Vehicles = [] };

        repo.Setup(x => x.UpdateAsync(5, It.IsAny<Employee>())).ReturnsAsync(true);
        var controller = new EmployeesController(repo.Object, cache.Object);

        var result = await controller.Update(5, input, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        repo.Verify(
            x => x.UpdateAsync(
                5,
                It.Is<Employee>(e => e.Id == 5 && e.FirstName == "X")),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenRepoMisses_ReturnsNotFound_AndStillCleansCache()
    {
        var repo = new Mock<IEmployeeRepository>();
        var cache = new Mock<ICarCache>();

        repo.Setup(x => x.DeleteAsync(3)).ReturnsAsync(false);
        var controller = new EmployeesController(repo.Object, cache.Object);

        var result = await controller.Delete(3, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
        cache.Verify(x => x.DeleteVehiclesForEmployeeAsync(3), Times.Once);
    }

    [Fact]
    public async Task PutVehicles_SetsCacheAndReturnsNoContent()
    {
        var repo = new Mock<IEmployeeRepository>();
        var cache = new Mock<ICarCache>();
        var vehicles = new List<Car> { new() { Id = 1, EmployeeId = 12, Fuel = "hybrid" } };
        var controller = new EmployeesController(repo.Object, cache.Object);

        var result = await controller.PutVehicles(12, vehicles, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        cache.Verify(x => x.SetVehiclesForEmployeeAsync(12, vehicles), Times.Once);
    }
}
