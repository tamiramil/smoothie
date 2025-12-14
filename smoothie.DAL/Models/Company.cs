using System.ComponentModel.DataAnnotations;

namespace smoothie.DAL.Models;

public class Company
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }

    public ICollection<Project> ProjectsAsCustomer { get; set; } = new List<Project>();
    public ICollection<Project> ProjectsAsExecutor { get; set; } = new List<Project>();
}