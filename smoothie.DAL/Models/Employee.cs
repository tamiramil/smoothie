using System.ComponentModel.DataAnnotations;

namespace smoothie.DAL.Models;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string? SecondName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public ICollection<Project> ManagedProjects  { get; set; } = new List<Project>();
    public ICollection<Project> AssignedProjects { get; set; } = new List<Project>();

    public string FullName => $"{FirstName} {SecondName}{(SecondName != null ? " " : "")}{LastName}";
}