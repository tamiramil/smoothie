using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using smoothie.BLL.DTOs;
using smoothie.BLL.Helpers;
using smoothie.BLL.Services;
using smoothie.DAL.Data;
using smoothie.DAL.Models;

namespace Smoothie.BLL.Services;

public class ProjectService : IProjectService
{
    private readonly SmoothieContext     _context;
    private readonly IWebHostEnvironment _environment;

    public ProjectService(SmoothieContext context, IWebHostEnvironment environment) {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task<List<Project>> GetAllAsync(ProjectFilterDto filter) {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

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
        if (projectDto is null)
            throw new ArgumentNullException(nameof(projectDto));

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

        ValidationHelper.ValidateOrThrow(project, nameof(project));

        if (projectDto.EmployeeIds is not null && projectDto.EmployeeIds.Any()) {
            var employees = await _context.Employees
                .Where(e => projectDto.EmployeeIds.Contains(e.Id))
                .ToListAsync();

            project.Employees = employees;
        }

        _context.Projects.Add(project);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try {
            await _context.SaveChangesAsync();

            if (files is not null && files.Any())
                await HandleFileUploadsAsync(project, files);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return project;
        } catch (IOException) {
            await transaction.RollbackAsync();
            throw;
        } catch (Exception) {
            await transaction.RollbackAsync();
            DeleteProjectFiles(project.Id);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Project project) {
        if (project is null)
            throw new ArgumentNullException(nameof(project));

        ValidationHelper.ValidateOrThrow(project, nameof(project));

        try {
            bool exists = await _context.Projects.AnyAsync(p => p.Id == project.Id);
            if (!exists)
                return false;

            _context.Update(project);
            await _context.SaveChangesAsync();
            return true;
        } catch {
            // No advanced exception handling for now
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
            // No advanced exception handling for now
            return false;
        }
    }

    public async Task<bool> ExistsAsync(int? id) {
        if (id is null || id <= 0)
            return false;

        return await _context.Projects.AnyAsync(p => p.Id == id);
    }

    private async Task HandleFileUploadsAsync(Project project, IList<IFormFile> files) {
        var relUploadPath = Path.Combine("uploads", "projects", project.Id.ToString());
        var absUploadPath = Path.Combine(_environment.WebRootPath, relUploadPath);

        Directory.CreateDirectory(absUploadPath);

        foreach (var file in files) {
            if (file.Length == 0)
                continue;

            var fileName = Path.GetFileName(file.FileName);
            var uniqueName = $"{Guid.NewGuid()}_{fileName}";
            var absFilePath = Path.Combine(absUploadPath, uniqueName);
            var relFilePath = Path.Combine(relUploadPath, uniqueName);

            try {
                await using (var stream = new FileStream(absFilePath, FileMode.Create)) {
                    await file.CopyToAsync(stream);
                }

                var document = new ProjectDocument {
                    ProjectId = project.Id,
                    FileName = fileName,
                    FilePath = relFilePath,
                    FileSize = file.Length
                };

                _context.Add(document);
            } catch (IOException e) {
                throw new IOException($"Error saving file {fileName}: {e.Message}");
            }
        }
    }

    private void DeleteProjectFiles(int projectId) {
        try {
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "projects", projectId.ToString());
            if (Directory.Exists(uploadPath)) {
                Directory.Delete(uploadPath, recursive: true);
            }
        } catch (Exception ex) {
            Console.WriteLine(
                $"WARNING: Failed to clean up files for Project ID {projectId} :(\n" +
                $"Manual cleanup required: {ex.Message}"
            );
        }
    }
}