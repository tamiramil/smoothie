using smoothie.BLL.DTOs;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

/// <summary>
/// Defines the contract for company-related operations, including CRUD operations and existence checks.
/// </summary>
public interface ICompanyService
{
    /// <summary>
    /// Retrieves all companies asynchronously.
    /// </summary>
    /// <param name="includeRelations">A boolean indicating whether related entities (e.g., ProjectsAsCustomer, ProjectsAsExecutor) should be included.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of all companies.</returns>
    Task<List<Company>> GetAllAsync(bool includeRelations = false);

    /// <summary>
    /// Retrieves a lightweight list of companies with summary information for index/listing views.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of company index DTOs with name and project counts.</returns>
    Task<List<CompanyIndexDto>> GetAllIndexAsync();

    /// <summary>
    /// Retrieves a company asynchronously by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the company.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the Company entity, or null if not found.</returns>
    Task<Company?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new company asynchronously.
    /// </summary>
    /// <param name="companyDto">The DTO containing company data (Name).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created Company entity, or null if creation failed.</returns>
    /// <exception cref="ArgumentException">Thrown if the resulting <see cref="Company"/> model is not valid according to data annotations.</exception>
    Task<Company?> CreateAsync(CompanyDto companyDto);

    /// <summary>
    /// Updates an existing company asynchronously.
    /// </summary>
    /// <param name="companyDto">The DTO containing updated company data.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the update was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="companyDto"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the <see cref="Company"/> model is not valid according to data annotations.</exception>
    Task<bool> UpdateAsync(CompanyDto companyDto);

    /// <summary>
    /// Deletes a company asynchronously by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the company to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks asynchronously if a company with the given unique identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the company to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the company exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(int? id);
}