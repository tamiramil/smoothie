using System.ComponentModel.DataAnnotations;

namespace smoothie.DAL.Models;

public class ProjectDocument
{
    public int Id { get; set; }

    [Required]
    public int      ProjectId { get; set; }
    public Project? Project   { get; set; }

    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long   FileSize { get; set; }
}