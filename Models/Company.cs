namespace smoothie.Models;

public class Company
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<Employee> Employees { get; set; }

    public ICollection<Project> ProjectsAsCustomer { get; set; } = new List<Project>();
    public ICollection<Project> ProjectsAsExecutor { get; set; } = new List<Project>();
}