using Microsoft.AspNetCore.Http;
using smoothie.BLL.DTOs;
using smoothie.DAL.Models;

namespace smoothie.BLL.Services;

/// <summary>
/// Defines the contract for project-related operations, including CRUD operations, filtering, and existence checks.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Retrieves a list of projects asynchronously based on the provided filter criteria.
    /// </summary>
    /// <param name="filter">The DTO containing criteria used to filter the projects.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of projects.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="filter"/> is null.</exception>
    Task<List<Project>> GetAllAsync(ProjectFilterDto filter);

    /// <summary>
    /// Retrieves a project asynchronously by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the project.</param>
    /// <param name="includeRelations">A boolean indicating whether related entities (e.g., Employees, Companies) should be included.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the Project entity, or null if not found.</returns>
    Task<Project?> GetByIdAsync(int id, bool includeRelations = false);

    /// <summary>
    /// Creates a new project asynchronously using data from the Project Wizard process.
    /// Wraps the entire operation (project creation and file uploads) in a database transaction.
    /// If file upload fails, the transaction is rolled back and uploaded files are cleaned up.
    /// </summary>
    /// <param name="projectDto">The DTO containing all required project data from the wizard steps.</param>
    /// <param name="files">An optional list of files to be associated and uploaded with the project.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created Project entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="projectDto"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the resulting <see cref="Project"/> model is not valid according to data annotations.</exception>
    /// <exception cref="IOException">Thrown if file upload operations fail. The transaction is rolled back automatically.</exception>
    /// <exception cref="DbUpdateException">Thrown if database operations fail. The transaction is rolled back and any uploaded files are deleted.</exception>
    Task<Project?> CreateAsync(ProjectWizardDto projectDto, IList<IFormFile>? files);

    /// <summary>
    /// Updates an existing project asynchronously.
    /// </summary>
    /// <param name="project">The Project entity with updated data.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the update was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="project"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the <see cref="Project"/> model is not valid according to data annotations.</exception>
    Task<bool> UpdateAsync(Project project);

    /// <summary>
    /// Deletes a project asynchronously by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the project to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(int? id);

    /// <summary>
    /// Checks asynchronously if a project with the given unique identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the project to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the project exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(int? id);
}