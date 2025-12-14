using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using smoothie.BLL.DTOs;
using smoothie.BLL.Services;
using smoothie.DAL.Models;
using smoothie.Web.ViewModels;

namespace Smoothie.Web.Controllers;

public class ProjectsController : Controller
{
    private const string WizardKey = "ProjectWizard";

    private readonly IProjectService  _projectService;
    private readonly ICompanyService  _companyService;
    private readonly IEmployeeService _employeeService;

    public ProjectsController(
        IProjectService projectService,
        ICompanyService companyService,
        IEmployeeService employeeService) {
        _projectService = projectService;
        _companyService = companyService;
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string sortOrder,
        DateTime? startDateFrom,
        DateTime? startDateTo,
        DateTime? endDateFrom,
        DateTime? endDateTo,
        int? priority,
        int? customerCompanyId,
        int? executorCompanyId) {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["StartDateSortParm"] = sortOrder == "start_date" ? "start_date_desc" : "start_date";
        ViewData["EndDateSortParm"] = sortOrder == "end_date" ? "end_date_desc" : "end_date";
        ViewData["PrioritySortParm"] = sortOrder == "priority" ? "priority_desc" : "priority";

        ViewBag.Companies = await GetCompaniesSelectListAsync();

        var filter = new ProjectFilterDto {
            StartDateFrom = startDateFrom,
            StartDateTo = startDateTo,
            EndDateFrom = endDateFrom,
            EndDateTo = endDateTo,
            Priority = priority,
            CustomerCompanyId = customerCompanyId,
            ExecutorCompanyId = executorCompanyId,
            SortOrder = sortOrder
        };

        var projects = await _projectService.GetAllAsync(filter);
        return View(projects);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id) {
        bool exists = await _projectService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var project = await _projectService.GetByIdAsync(id!.Value);
        ViewBag.AvailableEmployees = await _employeeService.GetByProjectIdAsync(id.Value);

        return View(project);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id) {
        bool exists = await _projectService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var project = await _projectService.GetByIdAsync(id!.Value, true);
        await PopulateEditDropDowns(project);
        return View(project);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Project project) {
        if (id != project.Id)
            return NotFound();

        if (!ModelState.IsValid) {
            await PopulateEditDropDowns(project);
            return View(project);
        }

        var success = await _projectService.UpdateAsync(project);
        if (!success)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id) {
        bool exists = await _projectService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var project = await _projectService.GetByIdAsync(id!.Value);
        return View(project);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        await _projectService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    #region Create Wizard

    [HttpGet]
    public IActionResult Create() {
        HttpContext.Session.Remove(WizardKey);
        return View(nameof(CreateStepA), new ProjectWizardViewModel());
    }

    [HttpGet]
    public IActionResult CreateStepA() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        model.CurrentStep = 1;
        return View(model);
    }

    [HttpPost]
    public IActionResult CreateStepA(ProjectWizardViewModel model) {
        if (!ModelState.IsValid)
            return View(model);
        
        SaveModel(model);
        return RedirectToAction(nameof(CreateStepB));
    }

    [HttpGet]
    public async Task<IActionResult> CreateStepB() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        ViewBag.Companies = await GetCompaniesSelectListAsync();

        model.CurrentStep = 2;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStepB(ProjectWizardViewModel model, string action) {
        var sessionModel = LoadModel();
        if (sessionModel is null)
            return RedirectToAction(nameof(Create));

        sessionModel.CustomerCompanyId = model.CustomerCompanyId;
        sessionModel.ExecutorCompanyId = model.ExecutorCompanyId;
        sessionModel.CurrentStep = 2;

        if (action == "back") {
            SaveModel(sessionModel);
            return RedirectToAction(nameof(CreateStepA));
        }

        ModelState.Clear();
        if (!TryValidateModel(sessionModel)) {
            ViewBag.Companies = await GetCompaniesSelectListAsync();
            return View(sessionModel);
        }

        SaveModel(sessionModel);
        return RedirectToAction(nameof(CreateStepC));
    }

    [HttpGet]
    public async Task<IActionResult> CreateStepC() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        ViewBag.Employees = await GetEmployeesSelectListAsync();

        model.CurrentStep = 3;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStepC(ProjectWizardViewModel model, string action) {
        var sessionModel = LoadModel();
        if (sessionModel is null)
            return RedirectToAction(nameof(Create));

        sessionModel.HeadId = model.HeadId;

        if (action == "back") {
            SaveModel(sessionModel);
            return RedirectToAction(nameof(CreateStepB));
        }

        ModelState.Clear();
        if (!TryValidateModel(sessionModel)) {
            ViewBag.Employees = await GetEmployeesSelectListAsync();
            return View(sessionModel);
        }

        SaveModel(sessionModel);
        return RedirectToAction(nameof(CreateStepD));
    }

    [HttpGet]
    public async Task<IActionResult> CreateStepD() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        ViewBag.Employees = await GetEmployeesSelectListAsync();

        model.CurrentStep = 4;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStepD(ProjectWizardViewModel model, string action) {
        var sessionModel = LoadModel();
        if (sessionModel is null)
            return RedirectToAction(nameof(Create));

        sessionModel.EmployeeIds = model.EmployeeIds ?? [];
        sessionModel.CurrentStep = 4;

        if (action == "back") {
            SaveModel(sessionModel);
            return RedirectToAction(nameof(CreateStepC));
        }

        ModelState.Clear();
        if (!TryValidateModel(sessionModel)) {
            ViewBag.Employees = await GetEmployeesSelectListAsync();
            return View(sessionModel);
        }

        if (!sessionModel.EmployeeIds.Contains(sessionModel.HeadId!.Value))
            sessionModel.EmployeeIds.Add(sessionModel.HeadId.Value);

        SaveModel(sessionModel);
        return RedirectToAction(nameof(CreateStepE));
    }

    [HttpGet]
    public IActionResult CreateStepE() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        model.CurrentStep = 5;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStepE(IList<IFormFile>? files, string action) {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        if (action == "back")
            return RedirectToAction(nameof(CreateStepD));
        
        try {
            var wizardDto = new ProjectWizardDto {
                Name = model.Name,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Priority = model.Priority,
                CustomerCompanyId = model.CustomerCompanyId,
                ExecutorCompanyId = model.ExecutorCompanyId,
                HeadId = model.HeadId,
                EmployeeIds = model.EmployeeIds
            };

            await _projectService.CreateAsync(wizardDto, files);
            HttpContext.Session.Remove(WizardKey);
            return RedirectToAction(nameof(Index));
        } catch {
            TempData["Error"] = "Failed to create project. Please try again.";
            return View(model);
        }
    }

    #endregion

    #region Helpers

    private async Task PopulateEditDropDowns(Project? project = null) {
        ViewData["CustomerCompanyId"] = await GetCompaniesSelectListAsync(project?.CustomerCompanyId);
        ViewData["ExecutorCompanyId"] = await GetCompaniesSelectListAsync(project?.ExecutorCompanyId);
        ViewData["HeadId"] = await GetEmployeesSelectListAsync(project?.HeadId);
    }

    private ProjectWizardViewModel? LoadModel() {
        var json = HttpContext.Session.GetString(WizardKey);
        return json is null ? null : JsonSerializer.Deserialize<ProjectWizardViewModel>(json);
    }

    private void SaveModel(ProjectWizardViewModel model) {
        var json = JsonSerializer.Serialize(model);
        HttpContext.Session.SetString(WizardKey, json);
    }

    private async Task<SelectList> GetCompaniesSelectListAsync(int? selectedId = null) {
        var companies = await _companyService.GetAllAsync();
        return new SelectList(
            companies,
            nameof(Company.Id),
            nameof(Company.Name),
            selectedId
        );
    }

    private async Task<SelectList> GetEmployeesSelectListAsync(int? selectedId = null) {
        var employees = await _employeeService.GetAllAsync();
        return new SelectList(
            employees,
            nameof(Employee.Id),
            nameof(Employee.FullName),
            selectedId
        );
    }

    #endregion
}