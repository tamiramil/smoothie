namespace smoothie.BLL.DTOs;

public class EmployeeIndexDto
{
    public int    Id       { get; set; }
    public string FullName { get; set; }
    public string Email    { get; set; }

    public int ManagedProjectsCount  { get; set; }
    public int AssignedProjectsCount { get; set; }
}