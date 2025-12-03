using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using smoothie.Models;

namespace smoothie.ViewModel;

public class ProjectWizardViewModel
{
    [Required(ErrorMessage = "Please enter a file name")]
    [StringLength(50, ErrorMessage = "The name length cannot exceed 50 characters")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Please enter the start date")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Required(ErrorMessage = "Please enter the end date")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Required(ErrorMessage = "Please enter the priority")]
    [Range(1, 10, ErrorMessage = "The priority must be between 1 and 10")]
    public int? Priority { get; set; }

    [Required(ErrorMessage = "Select suctomer company")]
    public int? CustomerCompanyId { get; set; }

    [Required(ErrorMessage = "Select executor company")]
    public int? ExecutorCompanyId { get; set; }

    [Required(ErrorMessage = "Please select the head of the project")]
    public int? HeadId { get; set; }

    public List<int>?       EmployeeIds       { get; set; } = new();
    public List<IFormFile>? UploadedFiles     { get; set; } = new();
    public List<string>?    ExistingFileNames { get; set; } = new();

    public int CurrentStep { get; set; } = 1;

    public SelectList?     Companies          { get; set; }
    public SelectList?     AllEmployees       { get; set; }
    public List<Employee>? AvailableEmployees { get; set; } = new();
}