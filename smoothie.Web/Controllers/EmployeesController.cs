using Microsoft.AspNetCore.Mvc;
using smoothie.BLL.DTOs;
using smoothie.BLL.Services;

namespace smoothie.Web.Controllers;

/// <summary>
/// Controller for managing employee-related operations in the web interface.
/// Provides endpoints for listing, viewing details, creating, editing, deleting, and searching employees.
/// </summary>
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeesController"/> class.
    /// </summary>
    /// <param name="employeeService">The service for employee-related business logic.</param>
    public EmployeesController(IEmployeeService employeeService) {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Displays the index page with a list of all employees.
    /// </summary>
    /// <returns>A view containing the list of employees with summary information (full name, email, project counts).</returns>
    [HttpGet]
    public async Task<IActionResult> Index() {
        var employees = await _employeeService.GetAllIndexAsync();
        return View(employees);
    }

    /// <summary>
    /// Displays detailed information about a specific employee, including their managed and assigned projects.
    /// </summary>
    /// <param name="id">The unique identifier of the employee.</param>
    /// <returns>A view with detailed employee information, or NotFound if the employee doesn't exist.</returns>
    [HttpGet]
    public async Task<IActionResult> Details(int? id) {
        bool exists = await _employeeService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var employee = await _employeeService.GetByIdAsync(id!.Value, true);
        return View(employee);
    }

    /// <summary>
    /// Displays the form for creating a new employee.
    /// </summary>
    /// <returns>A view with an empty employee DTO form.</returns>
    [HttpGet]
    public IActionResult Create() {
        var employeeDto = new EmployeeDto();
        return View(employeeDto);
    }

    /// <summary>
    /// Processes the creation of a new employee.
    /// </summary>
    /// <param name="employeeDto">The DTO containing the new employee data (FirstName, SecondName, LastName, Email).</param>
    /// <returns>Redirects to the Index page if successful; otherwise, returns the view with validation errors.</returns>
    [HttpPost]
    public async Task<IActionResult> Create(EmployeeDto employeeDto) {
        if (!ModelState.IsValid)
            return View(employeeDto);

        await _employeeService.CreateAsync(employeeDto);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Displays the form for editing an existing employee.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to edit.</param>
    /// <returns>A view with the employee DTO form populated with existing data, or NotFound if the employee doesn't exist.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int? id) {
        bool exists = await _employeeService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var employee = await _employeeService.GetByIdAsync(id!.Value);

        var employeeDto = new EmployeeDto {
            Id = employee!.Id,
            FirstName = employee.FirstName,
            SecondName = employee.SecondName,
            LastName = employee.LastName,
            Email = employee.Email
        };

        return View(employeeDto);
    }

    /// <summary>
    /// Processes the update of an existing employee.
    /// </summary>
    /// <param name="employeeDto">The DTO containing the updated employee data.</param>
    /// <returns>Redirects to the Index page if successful; returns NotFound if the employee doesn't exist; otherwise, returns the view with validation errors.</returns>
    [HttpPost]
    public async Task<IActionResult> Edit(EmployeeDto employeeDto) {
        if (!ModelState.IsValid)
            return View(employeeDto);

        var success = await _employeeService.UpdateAsync(employeeDto);
        if (!success)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Displays the confirmation page for deleting an employee.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to delete.</param>
    /// <returns>A view with employee details for deletion confirmation, or NotFound if the employee doesn't exist.</returns>
    [HttpGet]
    public async Task<IActionResult> Delete(int? id) {
        bool exists = await _employeeService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var employee = await _employeeService.GetByIdAsync(id!.Value);
        return View(employee);
    }

    /// <summary>
    /// Processes the deletion of an employee after confirmation.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to delete.</param>
    /// <returns>Redirects to the Index page after deletion.</returns>
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirm(int id) {
        await _employeeService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Searches for employees by a pattern matching their full name (case-insensitive, whitespace-ignored).
    /// </summary>
    /// <param name="pattern">The search pattern to match against employee names.</param>
    /// <returns>A JSON result containing a list of matching employees.</returns>
    [HttpGet]
    public async Task<IActionResult> SearchByPattern(string pattern) {
        var employees = await _employeeService.GetByPatternAsync(pattern);
        return Json(employees);
    }
}