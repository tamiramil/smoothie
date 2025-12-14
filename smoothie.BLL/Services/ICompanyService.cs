using smoothie.BLL.DTOs;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

public interface ICompanyService
{
    Task<List<Company>> GetAllAsync(bool includeRelations = false);
    Task<List<CompanyIndexDto>> GetAllIndexAsync();
    Task<Company?> GetByIdAsync(int id);
    Task<Company?> CreateAsync(CompanyDto companyDto);
    Task<bool> UpdateAsync(CompanyDto companyDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int? id);
}