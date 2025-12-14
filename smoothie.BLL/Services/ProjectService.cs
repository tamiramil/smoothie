using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using smoothie.BLL.DTOs;
using smoothie.BLL.Services;
using smoothie.DAL.Data;
using smoothie.DAL.Models;

namespace Smoothie.BLL.Services;

public class ProjectService : IProjectService
{
    private const int AttemptsNumber = 3;

    private readonly SmoothieContext     _context;
    private readonly IWebHostEnvironment _environment;

    public ProjectService(SmoothieContext context, IWebHostEnvironment environment) {
        _context = context;
        _environment = environment;
    }

    public async Task<List<Project>> GetAllAsync(ProjectFilterDto filter) {
        var projects = _context.Projects
            .Include(p => p.CustomerCompany)
            .Include(p => p.ExecutorCompany)
            .Include(p => p.Head)
            .Include(p => p.Employees)
            .Include(p => p.Documents)
            .AsQueryable();

        if (filter.StartDateFrom.HasValue)
            projects = projects.Where(p => p.StartDate >= filter.StartDateFrom);

        if (filter.StartDateTo.HasValue)
            projects = projects.Where(p => p.StartDate <= filter.StartDateTo);

        if (filter.EndDateFrom.HasValue)
            projects = projects.Where(p => p.EndDate >= filter.EndDateFrom);

        if (filter.EndDateTo.HasValue)
            projects = projects.Where(p => p.EndDate <= filter.EndDateTo);

        if (filter.Priority.HasValue)
            projects = projects.Where(p => p.Priority == filter.Priority.Value);

        if (filter.CustomerCompanyId.HasValue)
            projects = projects.Where(p => p.CustomerCompanyId == filter.CustomerCompanyId.Value);

        if (filter.ExecutorCompanyId.HasValue)
            projects = projects.Where(p => p.ExecutorCompanyId == filter.ExecutorCompanyId.Value);

        projects = filter.SortOrder switch {
            "name_desc" => projects.OrderByDescending(p => p.Name),
            "start_date" => projects.OrderBy(p => p.StartDate),
            "start_date_desc" => projects.OrderByDescending(p => p.StartDate),
            "end_date" => projects.OrderBy(p => p.EndDate),
            "end_date_desc" => projects.OrderByDescending(p => p.EndDate),
            "priority" => projects.OrderBy(p => p.Priority),
            "priority_desc" => projects.OrderByDescending(p => p.Priority),
            _ => projects.OrderBy(p => p.Name)
        };

        return await projects.ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id, bool includeRelations = false) {
        var projects = _context.Projects.AsQueryable();

        if (includeRelations)
            projects = projects
                .Include(p => p.CustomerCompany)
                .Include(p => p.ExecutorCompany)
                .Include(p => p.Employees)
                .Include(p => p.Head)
                .Include(p => p.Documents);

        return await projects.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project?> CreateAsync(ProjectWizardDto projectDto, IList<IFormFile>? files) {
        var project = new Project {
            Name = projectDto.Name!,
            StartDate = projectDto.StartDate!.Value,
            EndDate = projectDto.EndDate!.Value,
            Priority = projectDto.Priority!.Value,
            CustomerCompanyId = projectDto.CustomerCompanyId!.Value,
            ExecutorCompanyId = projectDto.ExecutorCompanyId!.Value,
            HeadId = projectDto.HeadId!.Value,
            Employees = new List<Employee>()
        };

        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(
            project,
            new ValidationContext(project),
            validationResults,
            true
        );

        if (!isValid)
            throw new ArgumentException(validationResults.ToString());

        if (projectDto.EmployeeIds is not null && projectDto.EmployeeIds.Any()) {
            var employees = await _context.Employees
                .Where(e => projectDto.EmployeeIds.Contains(e.Id))
                .ToListAsync();

            project.Employees = employees;
        }

        _context.Projects.Add(project);

        if (files is not null && files.Any())
            await HandleFileUploadsAsync(project, files);

        await SaveWithRetryAsync();
        return project;
    }

    public async Task<bool> UpdateAsync(Project project) {
        if (project is null)
            throw new ArgumentNullException(nameof(project));

        bool isValid = Validator.TryValidateObject(project, new ValidationContext(project), null, true);
        if (!isValid)
            throw new ArgumentException("Model provided is not valid");

        try {
            bool exists = await _context.Projects.AnyAsync(p => p.Id == project.Id);
            if (!exists)
                return false;

            _context.Update(project);
            await _context.SaveChangesAsync();
            return true;
        } catch {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int? id) {
        if (!await ExistsAsync(id))
            return false;

        var project = await GetByIdAsync(id!.Value, true);
        
        try {
            _context.Projects.Remove(project!);
            await _context.SaveChangesAsync();
            return true;
        } catch {
            return false;
        }
    }

    public async Task<bool> ExistsAsync(int? id) {
        if (id is null)
            return false;

        return await _context.Projects.AnyAsync(p => p.Id == id);
    }

    private async Task HandleFileUploadsAsync(Project project, IList<IFormFile> files) {
        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "projects", project.Id.ToString());
        Directory.CreateDirectory(uploadPath);

        foreach (var file in files) {
            if (file.Length == 0)
                continue;

            var fileName = Path.GetFileName(file.FileName);
            var uniqueName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadPath, uniqueName);

            await using (var stream = new FileStream(filePath, FileMode.Create)) {
                await file.CopyToAsync(stream);
            }

            var document = new ProjectDocument {
                Project = project,
                FileName = fileName,
                FilePath = $"/uploads/projects/{uniqueName}",
                FileSize = file.Length
            };

            _context.Add(document);
        }
    }

    private async Task SaveWithRetryAsync() {
        for (int i = 0; i < AttemptsNumber; ++i) {
            try {
                await _context.SaveChangesAsync();
                return;
            } catch (Exception e) {
                if (i == AttemptsNumber - 1)
                    throw;

                await Task.Delay(1000);
            }
        }
    }
}