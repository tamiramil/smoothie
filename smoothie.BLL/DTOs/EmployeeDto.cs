namespace smoothie.BLL.DTOs;

public class EmployeeDto
{
    public int     Id         { get; set; }
    public string  FirstName  { get; set; }
    public string? SecondName { get; set; }
    public string  LastName   { get; set; }
    public string  Email      { get; set; }

    public string FullName => $"{FirstName} {SecondName}{(SecondName != null ? " " : "")}{LastName}";
}