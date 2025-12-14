using smoothie.BLL.DTOs;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

public interface IEmployeeService
{
    Task<List<Employee>> GetAllAsync();
    Task<List<EmployeeIndexDto>> GetAllIndexAsync();
    Task<Employee?> GetByIdAsync(int id, bool includeRelations = false);
    Task<List<Employee>> GetByPatternAsync(string pattern, bool includeRelations = false);
    Task<List<Employee>> GetByProjectIdAsync(int projectId, bool includeRelations = false);
    Task<Employee?> CreateAsync(EmployeeDto employeeDto);
    Task<bool> UpdateAsync(int id, EmployeeDto employeeDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int? id);
}