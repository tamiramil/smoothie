using Microsoft.EntityFrameworkCore;
using smoothie.BLL.DTOs;
using smoothie.BLL.Helpers;
using smoothie.DAL.Data;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

///<inheritdoc/>
public class CompanyService : ICompanyService
{
    private readonly SmoothieContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeService"/> class.
    /// </summary>
    /// <param name="context">The database context for project operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    public CompanyService(SmoothieContext context) {
        _context = context;
    }

    ///<inheritdoc/>
    public async Task<List<Company>> GetAllAsync(bool includeRelations = false) {
        var companies = _context.Companies.AsQueryable();

        if (includeRelations) {
            companies = companies
                .Include(c => c.ProjectsAsCustomer)
                .Include(c => c.ProjectsAsExecutor);
        }

        return await companies.ToListAsync();
    }

    ///<inheritdoc/>
    public async Task<List<CompanyIndexDto>> GetAllIndexAsync() {
        return await _context.Companies.Select(c => new CompanyIndexDto {
                Id = c.Id,
                Name = c.Name!,
                ProjectsAsCustomerCount = c.ProjectsAsCustomer.Count(),
                ProjectsAsExecutorCount = c.ProjectsAsExecutor.Count()
            }
        ).ToListAsync();
    }

    ///<inheritdoc/>
    public async Task<Company?> GetByIdAsync(int id) {
        return await _context.Companies.FindAsync(id);
    }

    ///<inheritdoc/>
    public async Task<Company?> CreateAsync(CompanyDto companyDto) {
        var company = new Company {
            Name = companyDto.Name
        };

        ValidationHelper.ValidateOrThrow(company, nameof(company));

        try {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        } catch (Exception) {
            throw;
        }
    }

    ///<inheritdoc/>
    public async Task<bool> UpdateAsync(CompanyDto companyDto) {
        if (companyDto is null)
            throw new ArgumentNullException(nameof(companyDto));

        try {
            var company = await GetByIdAsync(companyDto.Id);
            if (company is null)
                return false;

            company.Name = companyDto.Name;

            ValidationHelper.ValidateOrThrow(company, nameof(company));

            _context.Update(company);
            await _context.SaveChangesAsync();
            return true;
        } catch (Exception) {
            // No advanced exception handling for now
            return false;
        }
    }

    ///<inheritdoc/>
    public async Task<bool> DeleteAsync(int id) {
        if (!await ExistsAsync(id))
            return false;

        var company = await GetByIdAsync(id);

        try {
            _context.Companies.Remove(company!);
            await _context.SaveChangesAsync();
            return true;
        } catch (Exception) {
            // No advanced exception handling for now
            return false;
        }
    }

    ///<inheritdoc/>
    public async Task<bool> ExistsAsync(int? id) {
        if (id is null)
            return false;

        return await _context.Companies.AnyAsync(c => c.Id == id);
    }
}