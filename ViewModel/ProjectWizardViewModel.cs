using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using smoothie.Models;

namespace smoothie.ViewModel;

public class ProjectWizardViewModel
{
    [StringLength(50, ErrorMessage = "The name length cannot exceed 50 characters")]
    public string? Name { get; set; }

    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Range(1, 10, ErrorMessage = "The priority must be between 1 and 10")]
    public int? Priority { get; set; }

    public int? CustomerCompanyId { get; set; }
    public int? ExecutorCompanyId { get; set; }
    public int? HeadId            { get; set; }

    public List<int>?       EmployeeIds       { get; set; } = new();
    public List<IFormFile>? UploadedFiles     { get; set; } = new();
    public List<string>?    ExistingFileNames { get; set; } = new();

    public int CurrentStep { get; set; } = 1;

    public SelectList? Companies { get; set; }
}