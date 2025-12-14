using System.ComponentModel.DataAnnotations;

namespace smoothie.DAL.Models;

public class Project : IValidatableObject
{
    public const int MinPriority = 1;
    public const int MaxPriority = 10;

    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string? Name { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [Range(MinPriority, MaxPriority)]
    public int Priority { get; set; }

    [Required]
    public int      CustomerCompanyId { get; set; }
    public Company? CustomerCompany   { get; set; }

    [Required]
    public int      ExecutorCompanyId { get; set; }
    public Company? ExecutorCompany   { get; set; }

    [Required]
    public int       HeadId { get; set; }
    public Employee? Head   { get; set; }

    public ICollection<Employee>?        Employees { get; set; }
    public ICollection<ProjectDocument>? Documents { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
        if (EndDate <= StartDate)
            yield return new ValidationResult("End date must be after start date", [nameof(EndDate)]);

        if (CustomerCompanyId == ExecutorCompanyId)
            yield return new ValidationResult(
                "Customer company must differ from executor company",
                [nameof(CustomerCompanyId)]
            );
    }
}