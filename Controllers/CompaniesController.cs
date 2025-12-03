using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smoothie.Data;
using smoothie.Models;

namespace smoothie.Controllers;

public class CompaniesController : Controller
{
    private readonly SmoothieContext _context;

    public CompaniesController(SmoothieContext context) {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index() {
        var companies = await _context.Companies.ToListAsync();
        return View(companies);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Company company) {
        if (ModelState.IsValid) {
            _context.Add(company);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        return View(company);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int? id) {
        if (id is null) {
            return RedirectToAction("Index");
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);

        if (company is null) {
            return NotFound();
        }
        
        return View(company);
    }

    public async Task<IActionResult> Edit(int id, Company company) {
        if (id != company.Id) {
            return NotFound();
        }

        if (!ModelState.IsValid) {
            return RedirectToAction("Index");
        }

        try {
            _context.Update(company);
            await _context.SaveChangesAsync();
        } catch (DbUpdateConcurrencyException) {
            if (!_context.Companies.Any(c => c.Id == company.Id)) {
                return NotFound();
            }
            throw;
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Delete(int? id) {
        if (id is null) {
            return NotFound();
        }

        var company = await _context.Companies.FirstOrDefaultAsync(e => e.Id == id);

        if (company is null) {
            return NotFound();
        }
        
        return View(company);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirm(int id) {
        var company = await _context.Employees.FindAsync(id);
        if (company is not null) {
            _context.Remove(company);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}