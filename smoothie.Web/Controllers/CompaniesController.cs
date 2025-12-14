using Microsoft.AspNetCore.Mvc;
using smoothie.BLL.DTOs;
using smoothie.BLL.Services;

namespace smoothie.Web.Controllers;

public class CompaniesController : Controller
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService) {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index() {
        var companies = await _companyService.GetAllIndexAsync();
        return View(companies);
    }

    [HttpGet]
    public IActionResult Create() {
        var model = new CompanyDto();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CompanyDto companyDto) {
        if (!ModelState.IsValid)
            return View(companyDto);

        await _companyService.CreateAsync(companyDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id) {
        bool exists = await _companyService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var company = await _companyService.GetByIdAsync(id!.Value);

        var companyDto = new CompanyDto {
            Id = company!.Id,
            Name = company.Name
        };

        return View(companyDto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CompanyDto companyDto) {
        if (!ModelState.IsValid)
            return View(companyDto);

        var success = await _companyService.UpdateAsync(companyDto);
        if (!success)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id) {
        bool exists = await _companyService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var company = await _companyService.GetByIdAsync(id!.Value);
        return View(company);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirm(int id) {
        await _companyService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}