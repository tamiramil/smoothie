namespace smoothie.Models;

public class Project
{
    public int      Id        { get; set; }
    public string   Name      { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate   { get; set; }
    public int      Priority  { get; set; }

    public int     CustomerCompanyId { get; set; }
    public Company CustomerCompany   { get; set; }
    public int     ExecutorCompanyId { get; set; }
    public Company ExecutorCompany   { get; set; }

    public int      HeadId { get; set; }
    public Employee Head   { get; set; }

    public ICollection<Employee> Employees { get; set; }
}