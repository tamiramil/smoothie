namespace smoothie.BLL.DTOs;

public class ProjectFilterDto
{
    public DateTime? StartDateFrom     { get; set; }
    public DateTime? StartDateTo       { get; set; }
    public DateTime? EndDateFrom       { get; set; }
    public DateTime? EndDateTo         { get; set; }
    public int?      Priority          { get; set; }
    public int?      CustomerCompanyId { get; set; }
    public int?      ExecutorCompanyId { get; set; }
    public string?   SortOrder         { get; set; }
}