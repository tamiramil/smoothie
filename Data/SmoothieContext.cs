using Microsoft.EntityFrameworkCore;
using smoothie.Models;

namespace smoothie.Data;

public sealed class SmoothieContext : DbContext
{
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Company>  Companies { get; set; } = null!;
    public DbSet<Project>  Projects  { get; set; } = null!;

    public DbSet<ProjectDocument> ProjectDocuments { get; set; } = null!;

    public SmoothieContext(DbContextOptions<SmoothieContext> options) : base(options) {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Company)
            .WithMany(c => c.Employees)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Company>().HasData(
            new Company { Id = 101, Name = "Smoothie" },
            new Company { Id = 102, Name = "F-and-K" },
            new Company { Id = 103, Name = "Pantheon" },
            new Company { Id = 104, Name = "Gray Book" }
        );

        modelBuilder.Entity<Employee>().HasData(
            new Employee {
                Id = 101, Name = "Bob", Surename = "Bobson", Email = "bob.bobson@example.com", CompanyId = 101
            }, new Employee {
                Id = 102, Name = "Alice", Surename = "Zoe", Email = "alice.braus@example.com", CompanyId = 101
            }, new Employee {
                Id = 103, Name = "Catalina", Surename = "Braus", Email = "catalina.braus@example.com", CompanyId = 102
            }, new Employee {
                Id = 104, Name = "Burt", Surename = "Ackermann", Email = "burt.ackermann@example.com", CompanyId = 102
            }, new Employee {
                Id = 105, Name = "Camille", Surename = "Sadies", Email = "camille.sadies@example.com", CompanyId = 103
            }, new Employee {
                Id = 106, Name = "Steven", Surename = "Einstein", Email = "steven.einstein@example.com", CompanyId = 104
            }, new Employee {
                Id = 107, Name = "Albert", Surename = "Hoking", Email = "albert.hoking@example.com", CompanyId = 104
            }
        );

        base.OnModelCreating(modelBuilder);
    }
}