namespace smoothie.BLL.DTOs;

public class ProjectWizardDto
{
    public string?    Name              { get; set; }
    public DateTime?  StartDate         { get; set; }
    public DateTime?  EndDate           { get; set; }
    public int?       Priority          { get; set; }
    public int?       CustomerCompanyId { get; set; }
    public int?       ExecutorCompanyId { get; set; }
    public int?       HeadId            { get; set; }
    public List<int>? EmployeeIds       { get; set; }
}