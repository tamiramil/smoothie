namespace smoothie.Models;

public class Employee
{
    public int     Id         { get; set; }
    public string  FirstName  { get; set; }
    public string? SecondName { get; set; }
    public string  LastName   { get; set; }
    public string  Email      { get; set; }

    public ICollection<Project> ManagedProjects  { get; set; }
    public ICollection<Project> AssignedProjects { get; set; }
}