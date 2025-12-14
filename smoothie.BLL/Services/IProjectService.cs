using Microsoft.AspNetCore.Http;
using smoothie.BLL.DTOs;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

public interface IProjectService
{
    Task<List<Project>> GetAllAsync(ProjectFilterDto filter);
    Task<Project?> GetByIdAsync(int id, bool includeRelations = false);
    Task<Project?> CreateAsync(ProjectWizardDto projectDto, IList<IFormFile>? files);
    Task<bool> UpdateAsync(Project project);
    Task<bool> DeleteAsync(int? id);
    Task<bool> ExistsAsync(int? id);
}