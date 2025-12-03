using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smoothie.Data;
using smoothie.Models;

namespace smoothie.Controllers;

public class EmployeesController : Controller
{
    private readonly SmoothieContext _context;

    public EmployeesController(SmoothieContext context) {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index() {
        var employees = _context.Employees.ToList();
        return View(employees);
    }

    [HttpPost]
    public async Task<IActionResult> Details(int? id) {
        if (id is null) {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.ManagedProjects)
            .Include(e => e.AssignedProjects)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return NotFound();

        return View(employee);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Employee employee) {
        if (ModelState.IsValid) {
            _context.Add(employee);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        return View(employee);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id) {
        if (id is null) {
            return NotFound();
        }

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null) {
            return NotFound();
        }

        return View(employee);
    }

    public async Task<IActionResult> Edit(int id, Employee employee) {
        if (id != employee.Id) {
            return NotFound();
        }

        if (!ModelState.IsValid) {
            return View(employee);
        }

        try {
            _context.Update(employee);
            await _context.SaveChangesAsync();
        } catch (DbUpdateConcurrencyException) {
            if (!_context.Employees.Any(e => e.Id == id))
                return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id) {
        if (id is null) {
            return NotFound();
        }

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);

        _context.Remove(employee);
        await _context.SaveChangesAsync();
        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> Delete(int id) {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is not null) {
            _context.Remove(employee);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}