using smoothie.BLL.DTOs;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

/// <summary>
/// Defines the contract for employee-related operations, including CRUD operations, filtering, and existence checks.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Retrieves all employees asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of all employees.</returns>
    Task<List<Employee>> GetAllAsync();

    /// <summary>
    /// Retrieves a lightweight list of employees with summary information for index/listing views.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of employee index DTOs with full name, email, and project counts.</returns>
    Task<List<EmployeeIndexDto>> GetAllIndexAsync();

    /// <summary>
    /// Retrieves an employee asynchronously by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the employee.</param>
    /// <param name="includeRelations">A boolean indicating whether related entities (e.g., ManagedProjects, AssignedProjects) should be included.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the Employee entity, or null if not found.</returns>
    Task<Employee?> GetByIdAsync(int id, bool includeRelations = false);

    /// <summary>
    /// Retrieves employees asynchronously by searching for a pattern in their full name (FirstName + SecondName + LastName).
    /// The pattern matching is case-insensitive and ignores whitespace.
    /// </summary>
    /// <param name="pattern">The search pattern to match against employee names. If null or whitespace, returns all employees.</param>
    /// <param name="includeRelations">A boolean indicating whether related entities (e.g., ManagedProjects, AssignedProjects) should be included.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of matching employees.</returns>
    Task<List<Employee>> GetByPatternAsync(string pattern, bool includeRelations = false);

    /// <summary>
    /// Retrieves all employees asynchronously who are assigned to a specific project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="includeRelations">A boolean indicating whether related entities (e.g., ManagedProjects, AssignedProjects) should be included.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of employees assigned to the project.</returns>
    Task<List<Employee>> GetByProjectIdAsync(int projectId, bool includeRelations = false);

    /// <summary>
    /// Creates a new employee asynchronously.
    /// </summary>
    /// <param name="employeeDto">The DTO containing employee data (FirstName, SecondName, LastName, Email).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created Employee entity, or null if creation failed.</returns>
    /// <exception cref="ArgumentException">Thrown if the resulting <see cref="Employee"/> model is not valid according to data annotations.</exception>
    Task<Employee?> CreateAsync(EmployeeDto employeeDto);

    /// <summary>
    /// Updates an existing employee asynchronously.
    /// </summary>
    /// <param name="employeeDto">The DTO containing updated employee data.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the update was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="employeeDto"/> is null.</exception>
    Task<bool> UpdateAsync(EmployeeDto employeeDto);

    /// <summary>
    /// Deletes an employee asynchronously by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks asynchronously if an employee with the given unique identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the employee exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(int? id);
}