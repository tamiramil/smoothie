using Microsoft.EntityFrameworkCore;
using smoothie.DAL.Models;

namespace smoothie.DAL.Data;

public sealed class SmoothieContext : DbContext
{
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Company>  Companies { get; set; } = null!;
    public DbSet<Project>  Projects  { get; set; } = null!;

    public DbSet<ProjectDocument> ProjectDocuments { get; set; } = null!;

    public SmoothieContext(DbContextOptions<SmoothieContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Project>()
            .HasOne(p => p.CustomerCompany)
            .WithMany(c => c.ProjectsAsCustomer)
            .HasForeignKey(p => p.CustomerCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.ExecutorCompany)
            .WithMany(c => c.ProjectsAsExecutor)
            .HasForeignKey(p => p.ExecutorCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Head)
            .WithMany(h => h.ManagedProjects)
            .HasForeignKey(p => p.HeadId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Employees)
            .WithMany(e => e.AssignedProjects);

        modelBuilder.Entity<Employee>()
            .HasMany(e => e.AssignedProjects)
            .WithMany(e => e.Employees);

        modelBuilder.Entity<ProjectDocument>()
            .HasOne(pd => pd.Project)
            .WithMany(p => p.Documents)
            .HasForeignKey(pd => pd.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Company>().HasData(
            new Company { Id = 101, Name = "Smoothie" },
            new Company { Id = 102, Name = "F-and-K" },
            new Company { Id = 103, Name = "Pantheon" },
            new Company { Id = 104, Name = "Gray Book" }
        );

        modelBuilder.Entity<Employee>().HasData(
            new Employee { Id = 101, FirstName = "Bob", LastName = "Bobson", Email = "bob.bobson@example.com" },
            new Employee { Id = 102, FirstName = "Alice", LastName = "Zoe", Email = "alice.braus@example.com" },
            new Employee { Id = 103, FirstName = "Catalina", LastName = "Braus", Email = "catalina.braus@example.com" },
            new Employee { Id = 104, FirstName = "Burt", LastName = "Ackermann", Email = "burt.ackermann@example.com" },
            new Employee { Id = 105, FirstName = "Camille", LastName = "Sadies", Email = "camille.sadies@example.com" },
            new Employee { Id = 106, FirstName = "Steven", LastName = "House", Email = "steven.house@example.com" },
            new Employee { Id = 107, FirstName = "Albert", LastName = "Hoking", Email = "albert.hoking@example.com" }
        );

        base.OnModelCreating(modelBuilder);
    }
}