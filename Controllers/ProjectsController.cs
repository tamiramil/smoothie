using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using smoothie.Data;
using smoothie.Models;
using smoothie.ViewModel;

namespace smoothie.Controllers;

public class ProjectsController : Controller
{
    private const string WizardKey = "ProjectWizard";

    private readonly SmoothieContext     _context;
    private readonly IWebHostEnvironment _environment;

    public ProjectsController(SmoothieContext context, IWebHostEnvironment environment) {
        _context = context;
        _environment = environment;
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
        int? executorCompanyId
    ) {
        ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["StartDateSortParm"] = sortOrder == "start_date" ? "start_date_desc" : "start_date";
        ViewData["EndDateSortParm"] = sortOrder == "end_date" ? "end_date_desc" : "end_date";
        ViewData["PrioritySortParm"] = sortOrder == "priority" ? "priority_desc" : "priority";

        ViewBag.Companies = await _context.Companies
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();

        var projects = _context.Projects
            .Include(p => p.CustomerCompany)
            .Include(p => p.ExecutorCompany)
            .Include(p => p.Head)
            .Include(p => p.Employees)
            .AsQueryable();

        if (startDateFrom.HasValue)
            projects = projects.Where(p => p.StartDate >= startDateFrom);

        if (startDateTo.HasValue)
            projects = projects.Where(p => p.StartDate <= startDateTo);

        if (endDateFrom.HasValue)
            projects = projects.Where(p => p.EndDate >= endDateFrom);

        if (endDateTo.HasValue)
            projects = projects.Where(p => p.EndDate <= endDateTo);

        if (priority.HasValue)
            projects = projects.Where(p => p.Priority == priority.Value);

        if (customerCompanyId.HasValue)
            projects = projects.Where(p => p.CustomerCompanyId == customerCompanyId.Value);

        if (executorCompanyId.HasValue)
            projects = projects.Where(p => p.ExecutorCompanyId == executorCompanyId.Value);

        projects = sortOrder switch {
            "name_desc" => projects.OrderByDescending(p => p.Name),
            "start_date" => projects.OrderBy(p => p.StartDate),
            "start_date_desc" => projects.OrderByDescending(p => p.StartDate),
            "end_date" => projects.OrderBy(p => p.EndDate),
            "end_date_desc" => projects.OrderByDescending(p => p.EndDate),
            "priority" => projects.OrderBy(p => p.Priority),
            "priority_desc" => projects.OrderByDescending(p => p.Priority),
            _ => projects.OrderBy(p => p.Name)
        };

        return View(await projects.ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id) {
        if (id == null)
            return NotFound();

        var project = _context.Projects
            .Include(p => p.CustomerCompany)
            .Include(p => p.ExecutorCompany)
            .Include(p => p.Head)
            .Include(p => p.Employees)
            .FirstOrDefault(p => p.Id == id);

        if (project == null)
            return NotFound();

        var availableEmployees = await _context.Employees
            .Where(e => e.AssignedProjects.Any(p => p.Id == id))
            .ToListAsync();

        ViewBag.AvailableEmployees = new SelectList(availableEmployees, "Id", "Name");

        return View(project);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id) {
        if (id is null) {
            return NotFound();
        }

        var project = await _context.Projects
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null) {
            return NotFound();
        }

        await PopulateEditDropDowns();
        return View(project);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int? id, Project project) {
        if (id != project.Id) {
            return NotFound();
        }

        if (ModelState.IsValid) {
            await PopulateEditDropDowns();
            return View(project);
        }

        try {
            _context.Update(project);
            await _context.SaveChangesAsync();
        } catch (DbUpdateConcurrencyException) {
            if (!_context.Projects.Any(p => p.Id == project.Id)) {
                return NotFound();
            }
            throw;
        }

        await PopulateEditDropDowns();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id) {
        if (id is null) {
            return NotFound();
        }

        var project = await _context.Projects
            .Include(p => p.CustomerCompany)
            .Include(p => p.ExecutorCompany)
            .Include(p => p.Head)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null) {
            return NotFound();
        }

        return View(project);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var project = await _context.Projects.FindAsync(id);
        if (project is not null) {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddEmployee(int? projectId, int? employeeId) {
        var project = await _context.Projects
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        var employee = await _context.Employees.FindAsync(employeeId);

        if (project == null || employee == null) {
            return NotFound();
        }

        if (!project.Employees.Contains(employee)) {
            project.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = project.Id });
    }

    [HttpPost]

    #region Create

    [HttpGet]
    public IActionResult Create() {
        HttpContext.Session.Remove(WizardKey);

        var model = new ProjectWizardViewModel { CurrentStep = 1 };
        return View("CreateStepA", model);
    }


    [HttpPost]
    public IActionResult CreateStepA(ProjectWizardViewModel model) {
        if (string.IsNullOrWhiteSpace(model.Name)) {
            ModelState.AddModelError("Name", "Project name is required");
        }
        if (!model.StartDate.HasValue) {
            ModelState.AddModelError("StartDate", "Start date is required");
        }
        if (!model.EndDate.HasValue) {
            ModelState.AddModelError("EndDate", "End date is required");
        }
        if (!model.Priority.HasValue || model.Priority < 1 || model.Priority > 10) {
            ModelState.AddModelError("Priority", "Priority must be between 1 and 10");
        }
        if (model.EndDate <= model.StartDate) {
            ModelState.AddModelError("EndDate", "End date must come after the start date");
        }

        if (!ModelState.IsValid) {
            return View();
        }

        SaveModel(model);
        return RedirectToAction(nameof(CreateStepB));
    }

    [HttpGet]
    public async Task<IActionResult> CreateStepB() {
        var model = LoadModel();
        if (model is null) {
            return RedirectToAction(nameof(Create));
        }

        model.CurrentStep = 2;
        await PopulateCompaniesDropdown(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStepB(ProjectWizardViewModel model, string action) {
        var sessionModel = LoadModel();
        if (sessionModel is null) {
            return RedirectToAction(nameof(Create));
        }

        sessionModel.CustomerCompanyId = model.CustomerCompanyId;
        sessionModel.ExecutorCompanyId = model.ExecutorCompanyId;
        sessionModel.CurrentStep = 2;

        if (action == "back") {
            SaveModel(sessionModel);
            return RedirectToAction(nameof(Create));
        }

        if (!sessionModel.CustomerCompanyId.HasValue) {
            ModelState.AddModelError("CustomerCompanyId", "Customer Company is required");
        }
        if (!sessionModel.ExecutorCompanyId.HasValue) {
            ModelState.AddModelError("ExecutorCompanyId", "Executor Company is required");
        }
        if (sessionModel.CustomerCompanyId == sessionModel.ExecutorCompanyId) {
            ModelState.AddModelError("CustomerCompanyId", "Customer and executor must be different");
        }

        if (!ModelState.IsValid) {
            await PopulateCompaniesDropdown(sessionModel);
            return View(sessionModel);
        }

        SaveModel(sessionModel);
        return RedirectToAction(nameof(CreateStepC));
    }

    [HttpGet]
    public async Task<IActionResult> CreateStepC() {
        var model = LoadModel();
        if (model is null) {
            return RedirectToAction(nameof(Create));
        }

        model.CurrentStep = 3;
        await PopulateEmployeesDropdown(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStepC(ProjectWizardViewModel model, string action) {
        var sessionModel = LoadModel();
        if (sessionModel is null) {
            return RedirectToAction(nameof(Create));
        }

        sessionModel.HeadId = model.HeadId;
        sessionModel.EmployeeIds = model.EmployeeIds;

        if (action == "back") {
            SaveModel(sessionModel);
            return RedirectToAction(nameof(CreateStepB));
        }

        if (!sessionModel.HeadId.HasValue) {
            ModelState.AddModelError("HeadId", "Head must be set");
            await PopulateEmployeesDropdown(sessionModel);
            return View(sessionModel);
        }

        SaveModel(sessionModel);
        return RedirectToAction(nameof(CreateStepD));
    }

    [HttpGet]
    public async Task<IActionResult> CreateStepD() {
        var model = LoadModel();
        if (model is null) {
            return RedirectToAction(nameof(Create));
        }

        model.CurrentStep = 4;
        model.AvailableEmployees = await _context.Employees.ToListAsync();

        return View(model);
    }

    [HttpPost]
    public IActionResult CreateStepD(ProjectWizardViewModel model, string action) {
        var sessionModel = LoadModel();
        if (sessionModel is null) {
            return RedirectToAction(nameof(Create));
        }

        sessionModel.EmployeeIds = model.EmployeeIds ?? new List<int>();
        sessionModel.CurrentStep = 4;

        if (action == "back") {
            SaveModel(sessionModel);
            return RedirectToAction(nameof(CreateStepC));
        }

        if (sessionModel.HeadId.HasValue && !sessionModel.EmployeeIds.Contains(sessionModel.HeadId.Value)) {
            sessionModel.EmployeeIds.Add(sessionModel.HeadId.Value);
        }

        SaveModel(sessionModel);
        return RedirectToAction(nameof(CreateStepE));
    }

    [HttpGet]
    public IActionResult CreateStepE() {
        var model = LoadModel();
        if (model is null) {
            return RedirectToAction(nameof(Create));
        }

        model.CurrentStep = 5;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStepE(IList<IFormFile>? files, string action) {
        var model = LoadModel();
        if (model is null) {
            return RedirectToAction(nameof(Create));
        }

        model.CurrentStep = 5;

        if (action == "back") {
            return RedirectToAction(nameof(CreateStepD));
        }

        var project = new Project {
            Name = model.Name!,
            StartDate = model.StartDate!.Value,
            EndDate = model.EndDate!.Value,
            Priority = model.Priority!.Value,
            CustomerCompanyId = model.CustomerCompanyId!.Value,
            ExecutorCompanyId = model.ExecutorCompanyId!.Value,
            HeadId = model.HeadId!.Value,
            Employees = new List<Employee>()
        };

        if (model.EmployeeIds is not null && model.EmployeeIds.Any()) {
            var employees = await _context.Employees
                .Where(e => model.EmployeeIds.Contains(e.Id))
                .ToListAsync();

            project.Employees = employees;
        }

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        if (files is not null && files.Any()) {
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "projects", project.Id.ToString());
            Directory.CreateDirectory(uploadPath);

            foreach (var file in files) {
                if (file.Length == 0) {
                    continue;
                }

                var fileName = Path.GetFileName(file.FileName);
                var uniqueName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(uploadPath, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create)) {
                    await file.CopyToAsync(stream);
                }

                var document = new ProjectDocument() {
                    ProjectId = project.Id,
                    FileName = fileName,
                    FilePath = $"/uploads/projects/{project.Id}/{uniqueName}",
                    FileSize = file.Length
                };

                _context.Add(document);
            }

            await _context.SaveChangesAsync();
        }

        HttpContext.Session.Remove(WizardKey);
        return RedirectToAction(nameof(Index));
    }

    #endregion

    private async Task PopulateEditDropDowns(Project? project = null) {
        ViewData["CustomerCompanyId"] = new SelectList(
            await _context.Companies.ToListAsync(), "Id", "Name", project?.CustomerCompanyId
        );

        ViewData["PerformerCompanyId"] = new SelectList(
            await _context.Companies.ToListAsync(), "Id", "Name", project?.ExecutorCompanyId
        );

        ViewData["HeadId"] = new SelectList(
            await _context.Employees.ToListAsync(), "Id", "Name", project?.HeadId
        );
    }

    #region Create helpers

    private ProjectWizardViewModel? LoadModel() {
        var json = HttpContext.Session.Get(WizardKey);
        return JsonSerializer.Deserialize<ProjectWizardViewModel>(json);
    }

    private void SaveModel(ProjectWizardViewModel model) {
        var json = JsonSerializer.Serialize(model);
        HttpContext.Session.SetString(WizardKey, json);
    }

    private async Task PopulateCompaniesDropdown(ProjectWizardViewModel model) {
        var companies = await _context.Companies.ToListAsync();
        model.Companies = new SelectList(companies, "Id", "Name");
    }

    private async Task PopulateEmployeesDropdown(ProjectWizardViewModel model) {
        var employees = await _context.Employees.ToListAsync();
        model.AllEmployees = new SelectList(employees, "Id", "Name");
    }

    #endregion
}