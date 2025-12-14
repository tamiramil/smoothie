using Microsoft.EntityFrameworkCore;
using smoothie.BLL.DTOs;
using smoothie.DAL.Data;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

public class EmployeeService : IEmployeeService
{
    private readonly SmoothieContext _context;

    public EmployeeService(SmoothieContext context) {
        _context = context;
    }

    public async Task<List<Employee>> GetAllAsync() {
        return await _context.Employees.ToListAsync();
    }

    public async Task<List<EmployeeIndexDto>> GetAllIndexAsync() {
        return await _context.Employees.Select(e => new EmployeeIndexDto {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                ManagedProjectsCount = e.ManagedProjects.Count,
                AssignedProjectsCount = e.AssignedProjects.Count,
            }
        ).ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id, bool includeRelations = false) {
        var employees = _context.Employees.AsQueryable();

        if (includeRelations)
            employees = employees
                .Include(e => e.ManagedProjects)
                .Include(e => e.AssignedProjects);

        return await employees.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Employee>> GetByPatternAsync(string pattern, bool includeRelations = false) {
        var employees = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pattern)) {
            pattern = pattern.ToLower().Replace(" ", "");
            employees = employees
                .Where(e => (e.FirstName + e.SecondName + e.LastName).ToLower().Contains(pattern));
        }

        if (includeRelations)
            employees = employees
                .Include(e => e.ManagedProjects)
                .Include(e => e.AssignedProjects);

        return await employees.ToListAsync();
    }

    public async Task<List<Employee>> GetByProjectIdAsync(int projectId, bool includeRelations = false) {
        var employees = _context.Employees.AsQueryable();

        if (includeRelations)
            employees = employees
                .Include(e => e.ManagedProjects)
                .Include(e => e.AssignedProjects);

        return await employees
            .Where(e => e.AssignedProjects.Any(p => p.Id == projectId))
            .ToListAsync();
    }

    public async Task<Employee?> CreateAsync(EmployeeDto employeeDto) {
        var employee = new Employee {
            FirstName = employeeDto.FirstName,
            SecondName = employeeDto.SecondName,
            LastName = employeeDto.LastName,
            Email = employeeDto.Email,
        };

        try {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        } catch {
            return null;
        }
    }

    public async Task<bool> UpdateAsync(int id, EmployeeDto employeeDto) {
        if (employeeDto is null)
            throw new ArgumentNullException(nameof(employeeDto));

        try {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee is null)
                return false;

            employee.FirstName = employeeDto.FirstName;
            employee.SecondName = employeeDto.SecondName;
            employee.LastName = employeeDto.LastName;
            employee.Email = employeeDto.Email;

            _context.Update(employee);
            await _context.SaveChangesAsync();
            return true;
        } catch {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id) {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
            return false;

        try {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        } catch {
            return false;
        }
    }

    public async Task<bool> ExistsAsync(int? id) {
        if (id is null)
            return false;

        return await _context.Employees.AnyAsync(e => e.Id == id);
    }
}