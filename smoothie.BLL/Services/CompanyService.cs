using Microsoft.EntityFrameworkCore;
using smoothie.BLL.DTOs;
using smoothie.DAL.Data;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

public class CompanyService : ICompanyService
{
    private readonly SmoothieContext _context;

    public CompanyService(SmoothieContext context) {
        _context = context;
    }

    public async Task<List<Company>> GetAllAsync(bool includeRelations = false) {
        var companies = _context.Companies.AsQueryable();

        if (includeRelations) {
            companies = companies
                .Include(c => c.ProjectsAsCustomer)
                .Include(c => c.ProjectsAsExecutor);
        }

        return await companies.ToListAsync();
    }

    public async Task<List<CompanyIndexDto>> GetAllIndexAsync() {
        return await _context.Companies.Select(c => new CompanyIndexDto {
                Id = c.Id,
                Name = c.Name!,
                ProjectsAsCustomerCount = c.ProjectsAsCustomer.Count(),
                ProjectsAsExecutorCount = c.ProjectsAsExecutor.Count()
            }
        ).ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int id) {
        return await _context.Companies.FindAsync(id);
    }

    public async Task<Company?> CreateAsync(CompanyDto companyDto) {
        var company = new Company {
            Name = companyDto.Name
        };

        try {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        } catch {
            return null;
        }
    }

    public async Task<bool> UpdateAsync(CompanyDto companyDto) {
        if (companyDto is null)
            throw new ArgumentNullException(nameof(companyDto));

        try {
            var company = await _context.Companies.FirstOrDefaultAsync(e => e.Id == companyDto.Id);
            if (company is null)
                return false;

            company.Name = companyDto.Name;

            _context.Update(company);
            await _context.SaveChangesAsync();
            return true;
        } catch {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id) {
        var company = await _context.Companies.FindAsync(id);
        if (company is null)
            return false;

        try {
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return true;
        } catch {
            return false;
        }
    }

    public async Task<bool> ExistsAsync(int? id) {
        if (id is null)
            return false;

        return await _context.Companies.AnyAsync(e => e.Id == id);
    }
}