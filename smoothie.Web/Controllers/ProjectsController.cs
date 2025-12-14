using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using smoothie.BLL.DTOs;
using smoothie.BLL.Services;
using smoothie.DAL.Models;
using smoothie.Web.ViewModels;

namespace Smoothie.Web.Controllers;

/// <summary>
/// Controller for managing project-related operations in the web interface.
/// Provides endpoints for listing, viewing details, editing, deleting projects,
/// and a multi-step wizard for creating new projects.
/// </summary>
public class ProjectsController : Controller
{
    private const string WizardKey = "ProjectWizard";

    private readonly IProjectService  _projectService;
    private readonly ICompanyService  _companyService;
    private readonly IEmployeeService _employeeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectsController"/> class.
    /// </summary>
    /// <param name="projectService">The service for project-related business logic.</param>
    /// <param name="companyService">The service for company-related business logic.</param>
    /// <param name="employeeService">The service for employee-related business logic.</param>
    public ProjectsController(
        IProjectService projectService,
        ICompanyService companyService,
        IEmployeeService employeeService) {
        _projectService = projectService;
        _companyService = companyService;
        _employeeService = employeeService;
    }

    /// <summary>
    /// Displays the index page with a filterable and sortable list of all projects.
    /// </summary>
    /// <param name="sortOrder">The column and direction to sort by (e.g., "name_desc", "start_date").</param>
    /// <param name="startDateFrom">The minimum start date for filtering projects.</param>
    /// <param name="startDateTo">The maximum start date for filtering projects.</param>
    /// <param name="endDateFrom">The minimum end date for filtering projects.</param>
    /// <param name="endDateTo">The maximum end date for filtering projects.</param>
    /// <param name="priority">The priority level for filtering projects.</param>
    /// <param name="customerCompanyId">The customer company ID for filtering projects.</param>
    /// <param name="executorCompanyId">The executor company ID for filtering projects.</param>
    /// <returns>A view containing the filtered and sorted list of projects.</returns>
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

    /// <summary>
    /// Displays detailed information about a specific project, including assigned employees.
    /// </summary>
    /// <param name="id">The unique identifier of the project.</param>
    /// <returns>A view with detailed project information, or NotFound if the project doesn't exist.</returns>
    [HttpGet]
    public async Task<IActionResult> Details(int? id) {
        bool exists = await _projectService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var project = await _projectService.GetByIdAsync(id!.Value);
        ViewBag.AvailableEmployees = await _employeeService.GetByProjectIdAsync(id.Value);

        return View(project);
    }

    /// <summary>
    /// Displays the form for editing an existing project.
    /// </summary>
    /// <param name="id">The unique identifier of the project to edit.</param>
    /// <returns>A view with the project form populated with existing data and related entities, or NotFound if the project doesn't exist.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int? id) {
        bool exists = await _projectService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var project = await _projectService.GetByIdAsync(id!.Value, true);
        await PopulateEditDropDowns(project);
        return View(project);
    }

    /// <summary>
    /// Processes the update of an existing project.
    /// </summary>
    /// <param name="id">The unique identifier of the project being edited.</param>
    /// <param name="project">The project entity containing the updated data.</param>
    /// <returns>Redirects to the Index page if successful; returns NotFound if the project doesn't exist or ID mismatch occurs; otherwise, returns the view with validation errors.</returns>
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

    /// <summary>
    /// Displays the confirmation page for deleting a project.
    /// </summary>
    /// <param name="id">The unique identifier of the project to delete.</param>
    /// <returns>A view with project details for deletion confirmation, or NotFound if the project doesn't exist.</returns>
    [HttpGet]
    public async Task<IActionResult> Delete(int? id) {
        bool exists = await _projectService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        var project = await _projectService.GetByIdAsync(id!.Value);
        return View(project);
    }

    /// <summary>
    /// Processes the deletion of a project after confirmation.
    /// </summary>
    /// <param name="id">The unique identifier of the project to delete.</param>
    /// <returns>Redirects to the Index page after deletion.</returns>
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        await _projectService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    #region Create Wizard

    /// <summary>
    /// Initializes the project creation wizard by clearing any existing session data.
    /// </summary>
    /// <returns>Redirects to the first step of the wizard (CreateStepA).</returns>
    [HttpGet]
    public IActionResult Create() {
        HttpContext.Session.Remove(WizardKey);
        return View(nameof(CreateStepA), new ProjectWizardViewModel());
    }

    /// <summary>
    /// Displays Step A of the project creation wizard for entering basic project information (name, dates, priority).
    /// </summary>
    /// <returns>A view with the wizard form for Step A, or redirects to Create if no session data exists.</returns>
    [HttpGet]
    public IActionResult CreateStepA() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        model.CurrentStep = 1;
        return View(model);
    }

    /// <summary>
    /// Processes Step A of the project creation wizard and advances to Step B.
    /// </summary>
    /// <param name="model">The view model containing basic project information.</param>
    /// <returns>Redirects to Step B if validation succeeds; otherwise, returns the view with validation errors.</returns>
    [HttpPost]
    public IActionResult CreateStepA(ProjectWizardViewModel model) {
        if (!ModelState.IsValid)
            return View(model);
        
        SaveModel(model);
        return RedirectToAction(nameof(CreateStepB));
    }

    /// <summary>
    /// Displays Step B of the project creation wizard for selecting customer and executor companies.
    /// </summary>
    /// <returns>A view with the wizard form for Step B and company select lists, or redirects to Create if no session data exists.</returns>
    [HttpGet]
    public async Task<IActionResult> CreateStepB() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        ViewBag.Companies = await GetCompaniesSelectListAsync();

        model.CurrentStep = 2;
        return View(model);
    }

    /// <summary>
    /// Processes Step B of the project creation wizard.
    /// </summary>
    /// <param name="model">The view model containing selected company IDs.</param>
    /// <param name="action">The action to perform ("back" to return to Step A, or proceed to Step C).</param>
    /// <returns>Redirects to Step A if action is "back", redirects to Step C if validation succeeds; otherwise, returns the view with validation errors.</returns>
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

    /// <summary>
    /// Displays Step C of the project creation wizard for selecting the project head (manager).
    /// </summary>
    /// <returns>A view with the wizard form for Step C and employee select lists, or redirects to Create if no session data exists.</returns>
    [HttpGet]
    public async Task<IActionResult> CreateStepC() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        ViewBag.Employees = await GetEmployeesSelectListAsync();

        model.CurrentStep = 3;
        return View(model);
    }

    /// <summary>
    /// Processes Step C of the project creation wizard.
    /// </summary>
    /// <param name="model">The view model containing the selected head employee ID.</param>
    /// <param name="action">The action to perform ("back" to return to Step B, or proceed to Step D).</param>
    /// <returns>Redirects to Step B if action is "back", redirects to Step D if validation succeeds; otherwise, returns the view with validation errors.</returns>
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

    /// <summary>
    /// Displays Step D of the project creation wizard for selecting project team members.
    /// </summary>
    /// <returns>A view with the wizard form for Step D and employee select lists, or redirects to Create if no session data exists.</returns>
    [HttpGet]
    public async Task<IActionResult> CreateStepD() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        ViewBag.Employees = await GetEmployeesSelectListAsync();

        model.CurrentStep = 4;
        return View(model);
    }

    /// <summary>
    /// Processes Step D of the project creation wizard. Automatically adds the project head to the team if not already included.
    /// </summary>
    /// <param name="model">The view model containing the selected employee IDs.</param>
    /// <param name="action">The action to perform ("back" to return to Step C, or proceed to Step E).</param>
    /// <returns>Redirects to Step C if action is "back", redirects to Step E if validation succeeds; otherwise, returns the view with validation errors.</returns>
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

    /// <summary>
    /// Displays Step E (final step) of the project creation wizard for uploading project files.
    /// </summary>
    /// <returns>A view with the wizard form for Step E, or redirects to Create if no session data exists.</returns>
    [HttpGet]
    public IActionResult CreateStepE() {
        var model = LoadModel();
        if (model is null)
            return RedirectToAction(nameof(Create));

        model.CurrentStep = 5;
        return View(model);
    }

    /// <summary>
    /// Processes Step E (final step) of the project creation wizard and creates the project with uploaded files.
    /// Clears the wizard session data upon successful creation.
    /// </summary>
    /// <param name="files">Optional list of files to upload with the project.</param>
    /// <param name="action">The action to perform ("back" to return to Step D, or submit to create the project).</param>
    /// <returns>Redirects to Step D if action is "back", redirects to Index if creation succeeds; otherwise, returns the view with an error message.</returns>
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

    /// <summary>
    /// Populates dropdown lists (SelectLists) for the Edit view with companies and employees.
    /// </summary>
    /// <param name="project">Optional project entity to pre-select values in the dropdowns.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task PopulateEditDropDowns(Project? project = null) {
        ViewData["CustomerCompanyId"] = await GetCompaniesSelectListAsync(project?.CustomerCompanyId);
        ViewData["ExecutorCompanyId"] = await GetCompaniesSelectListAsync(project?.ExecutorCompanyId);
        ViewData["HeadId"] = await GetEmployeesSelectListAsync(project?.HeadId);
    }

    /// <summary>
    /// Loads the project wizard view model from the current session.
    /// </summary>
    /// <returns>The deserialized wizard view model, or null if no session data exists.</returns>
    private ProjectWizardViewModel? LoadModel() {
        var json = HttpContext.Session.GetString(WizardKey);
        return json is null ? null : JsonSerializer.Deserialize<ProjectWizardViewModel>(json);
    }

    /// <summary>
    /// Saves the project wizard view model to the current session.
    /// </summary>
    /// <param name="model">The wizard view model to serialize and store.</param>
    private void SaveModel(ProjectWizardViewModel model) {
        var json = JsonSerializer.Serialize(model);
        HttpContext.Session.SetString(WizardKey, json);
    }

    /// <summary>
    /// Creates a SelectList of all companies for dropdown controls.
    /// </summary>
    /// <param name="selectedId">Optional ID of the company to pre-select in the list.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a SelectList of companies.</returns>
    private async Task<SelectList> GetCompaniesSelectListAsync(int? selectedId = null) {
        var companies = await _companyService.GetAllAsync();
        return new SelectList(
            companies,
            nameof(Company.Id),
            nameof(Company.Name),
            selectedId
        );
    }

    /// <summary>
    /// Creates a SelectList of all employees for dropdown controls.
    /// </summary>
    /// <param name="selectedId">Optional ID of the employee to pre-select in the list.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a SelectList of employees.</returns>
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