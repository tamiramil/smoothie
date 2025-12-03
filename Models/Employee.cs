namespace smoothie.Models;

public class Employee
{
    public int     Id         { get; set; }
    public string  Name       { get; set; }
    public string  Surename   { get; set; }
    public string? Patronymic { get; set; }
    public string  Email      { get; set; }

    public ICollection<Project> ManagedProjects  { get; set; }
    public ICollection<Project> AssignedProjects { get; set; }
}