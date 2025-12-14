using Microsoft.AspNetCore.Mvc;
using smoothie.BLL.DTOs;
using smoothie.BLL.Services;

namespace smoothie.Web.Controllers;

/// <summary>
/// Controller for managing company-related operations in the web interface.
/// Provides endpoints for listing, creating, editing, and deleting companies.
/// </summary>
public class CompaniesController : Controller
{
    private readonly ICompanyService _companyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompaniesController"/> class.
    /// </summary>
    /// <param name="companyService">The service for company-related business logic.</param>
    public CompaniesController(ICompanyService companyService) {
        _companyService = companyService;
    }

    /// <summary>
    /// Displays the index page with a list of all companies.
    /// </summary>
    /// <returns>A view containing the list of companies with summary information.</returns>
    [HttpGet]
    public async Task<IActionResult> Index() {
        var companies = await _companyService.GetAllIndexAsync();
        return View(companies);
    }

    /// <summary>
    /// Displays the form for creating a new company.
    /// </summary>
    /// <returns>A view with an empty company DTO form.</returns>
    [HttpGet]
    public IActionResult Create() {
        var model = new CompanyDto();
        return View(model);
    }

    /// <summary>
    /// Processes the creation of a new company.
    /// </summary>
    /// <param name="companyDto">The DTO containing the new company data.</param>
    /// <returns>Redirects to the Index page if successful; otherwise, returns the view with validation errors.</returns>
    [HttpPost]
    public async Task<IActionResult> Create(CompanyDto companyDto) {
        if (!ModelState.IsValid)
            return View(companyDto);

        await _companyService.CreateAsync(companyDto);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Displays the form for editing an existing company.
    /// </summary>
    /// <param name="id">The unique identifier of the company to edit.</param>
    /// <returns>A view with the company DTO form populated with existing data, or NotFound if the company doesn't exist.</returns>
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

    /// <summary>
    /// Processes the update of an existing company.
    /// </summary>
    /// <param name="companyDto">The DTO containing the updated company data.</param>
    /// <returns>Redirects to the Index page if successful; returns NotFound if the company doesn't exist; otherwise, returns the view with validation errors.</returns>
    [HttpPost]
    public async Task<IActionResult> Edit(CompanyDto companyDto) {
        if (!ModelState.IsValid)
            return View(companyDto);

        var success = await _companyService.UpdateAsync(companyDto);
        if (!success)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Displays the confirmation page for deleting a company.
    /// </summary>
    /// <param name="id">The unique identifier of the company to delete.</param>
    /// <returns>A view with company details for deletion confirmation, or NotFound if the company doesn't exist.</returns>
    [HttpGet]
    public async Task<IActionResult> Delete(int? id) {
        bool exists = await _companyService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var company = await _companyService.GetByIdAsync(id!.Value);
        return View(company);
    }

    /// <summary>
    /// Processes the deletion of a company after confirmation.
    /// </summary>
    /// <param name="id">The unique identifier of the company to delete.</param>
    /// <returns>Redirects to the Index page after deletion.</returns>
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirm(int id) {
        await _companyService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}