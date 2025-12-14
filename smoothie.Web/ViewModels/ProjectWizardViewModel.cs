using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using smoothie.DAL.Models;

namespace smoothie.Web.ViewModels;

public class ProjectWizardViewModel : IValidatableObject
{
    [StringLength(50, ErrorMessage = "Project name is too long")]
    public string? Name { get; set; }

    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Range(Project.MinPriority, Project.MaxPriority, ErrorMessage = $"Priority is invalid")]
    public int? Priority { get; set; }

    public int? CustomerCompanyId { get; set; }
    public int? ExecutorCompanyId { get; set; }
    public int? HeadId            { get; set; }

    public List<int>? EmployeeIds { get; set; } = new();

    public int CurrentStep { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
        if (string.IsNullOrWhiteSpace(Name))
            yield return new ValidationResult("Project name is required", [nameof(Name)]);

        if (!StartDate.HasValue)
            yield return new ValidationResult("Start date is required", [nameof(StartDate)]);

        if (!EndDate.HasValue)
            yield return new ValidationResult("End date is required", [nameof(EndDate)]);

        if (!Priority.HasValue)
            yield return new ValidationResult("Priority value is required", [nameof(Priority)]);

        if (EndDate <= StartDate)
            yield return new ValidationResult("End date must be after start date", [nameof(EndDate)]);

        if (CurrentStep == 1)
            yield break;

        if (!CustomerCompanyId.HasValue)
            yield return new ValidationResult("Customer company is required", [nameof(CustomerCompanyId)]);

        if (!ExecutorCompanyId.HasValue)
            yield return new ValidationResult("Executor company is required", [nameof(ExecutorCompanyId)]);

        if (CustomerCompanyId.HasValue && ExecutorCompanyId.HasValue && CustomerCompanyId == ExecutorCompanyId)
            yield return new ValidationResult("Executor must differ from customer", [nameof(ExecutorCompanyId)]);

        if (CurrentStep == 2)
            yield break;

        if (!HeadId.HasValue)
            yield return new ValidationResult("Head must be specified", [nameof(HeadId)]);

    }
}