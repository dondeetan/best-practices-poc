using Api.Entities;
using Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeRepository _repo;
    private readonly ICarCache _vehicleCache;

    public EmployeesController(IEmployeeRepository repo, ICarCache carCache)
    {
        _repo = repo;
        _vehicleCache = carCache;
    }

    // GET /api/employees/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Employee>> GetById(int id, CancellationToken ct)
    {
        var employee = await _repo.GetAsync(id);
        if (employee is null) return NotFound();

        employee.Vehicles = await _vehicleCache.GetVehiclesForEmployeeAsync(id);
        return Ok(employee);
    }

    // POST /api/employees
    [HttpPost]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status201Created)]
    public async Task<ActionResult<Employee>> Create([FromBody] Employee employee, CancellationToken ct)
    {
        var created = await _repo.CreateAsync(employee);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/employees/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Employee employee, CancellationToken ct)
    {
        employee.Id = id; // ensure path id wins
        var ok = await _repo.UpdateAsync(id, employee);
        return ok ? NoContent() : NotFound();
    }

    // DELETE /api/employees/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _vehicleCache.DeleteVehiclesForEmployeeAsync(id); // optional cache cleanup
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    // GET /api/employees/{id}/vehicles
    [HttpGet("{id:int}/vehicles")]
    [ProducesResponseType(typeof(List<Car>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Car>>> GetVehicles(int id, CancellationToken ct)
    {
        var vehicles = await _vehicleCache.GetVehiclesForEmployeeAsync(id);
        return Ok(vehicles);
    }

    // PUT /api/employees/{id}/vehicles
    [HttpPut("{id:int}/vehicles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PutVehicles(int id, [FromBody] List<Car> vehicles, CancellationToken ct)
    {
        await _vehicleCache.SetVehiclesForEmployeeAsync(id, vehicles);
        return NoContent();
    }
}
