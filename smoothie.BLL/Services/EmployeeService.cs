using Microsoft.EntityFrameworkCore;
using smoothie.BLL.DTOs;
using smoothie.BLL.Helpers;
using smoothie.DAL.Data;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

///<inheritdoc/>
public class EmployeeService : IEmployeeService
{
    private readonly SmoothieContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeService"/> class.
    /// </summary>
    /// <param name="context">The database context for project operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    public EmployeeService(SmoothieContext context) {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    ///<inheritdoc/>
    public async Task<List<Employee>> GetAllAsync() {
        return await _context.Employees.ToListAsync();
    }

    ///<inheritdoc/>
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

    ///<inheritdoc/>
    public async Task<Employee?> GetByIdAsync(int id, bool includeRelations = false) {
        var employees = _context.Employees.AsQueryable();

        if (includeRelations)
            employees = employees
                .Include(e => e.ManagedProjects)
                .Include(e => e.AssignedProjects);

        return await employees.FirstOrDefaultAsync(e => e.Id == id);
    }

    ///<inheritdoc/>
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

    ///<inheritdoc/>
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

    ///<inheritdoc/>
    public async Task<Employee?> CreateAsync(EmployeeDto employeeDto) {
        var employee = new Employee {
            FirstName = employeeDto.FirstName,
            SecondName = employeeDto.SecondName,
            LastName = employeeDto.LastName,
            Email = employeeDto.Email,
        };

        ValidationHelper.ValidateOrThrow(employee, nameof(employee));

        try {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        } catch (Exception e) {
            throw;
        }
    }

    ///<inheritdoc/>
    public async Task<bool> UpdateAsync(EmployeeDto employeeDto) {
        if (employeeDto is null)
            throw new ArgumentNullException(nameof(employeeDto));

        try {
            var employee = await GetByIdAsync(employeeDto.Id);
            if (employee is null)
                return false;

            employee.FirstName = employeeDto.FirstName;
            employee.SecondName = employeeDto.SecondName;
            employee.LastName = employeeDto.LastName;
            employee.Email = employeeDto.Email;

            _context.Update(employee);
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

        var employee = await GetByIdAsync(id);

        try {
            _context.Employees.Remove(employee!);
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

        return await _context.Employees.AnyAsync(e => e.Id == id);
    }
}