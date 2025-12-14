namespace smoothie.BLL.DTOs;

public class CompanyIndexDto
{
    public int    Id   { get; set; }
    public string Name { get; set; }

    public int ProjectsAsCustomerCount { get; set; }
    public int ProjectsAsExecutorCount { get; set; }
}