using Microsoft.AspNetCore.Mvc;
using smoothie.BLL.DTOs;
using smoothie.BLL.Services;

namespace smoothie.Web.Controllers;

public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService) {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index() {
        var employees = await _employeeService.GetAllIndexAsync();
        return View(employees);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id) {
        bool exists = await _employeeService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var employee = await _employeeService.GetByIdAsync(id!.Value, true);
        return View(employee);
    }

    [HttpGet]
    public IActionResult Create() {
        var employeeDto = new EmployeeDto();
        return View(employeeDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmployeeDto employeeDto) {
        if (!ModelState.IsValid)
            return View(employeeDto);

        await _employeeService.CreateAsync(employeeDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id) {
        bool exists = await _employeeService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var employee = await _employeeService.GetByIdAsync(id!.Value);

        var employeeDto = new EmployeeDto {
            FirstName = employee?.FirstName,
            SecondName = employee?.SecondName,
            LastName = employee?.LastName,
            Email = employee?.Email
        };

        return View(employeeDto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EmployeeDto employeeDto) {
        if (!ModelState.IsValid)
            return View(employeeDto);

        var success = await _employeeService.UpdateAsync(id, employeeDto);
        if (!success)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id) {
        bool exists = await _employeeService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var employee = await _employeeService.GetByIdAsync(id!.Value);
        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirm(int id) {
        await _employeeService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> SearchByPattern(string pattern) {
        var employees = await _employeeService.GetByPatternAsync(pattern);
        return Json(employees);
    }
}